using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Cirrus.Base.Services.Abstract;
using Cirrus.Models.Business.Lyrics;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Serialization;
using Cirrus.Models.Shared.User;
using Microsoft.Data.Sqlite;

namespace Cirrus.Data;

public class SqliteCacheService : ICacheService
{
    private readonly string _conStr;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    
    public SqliteCacheService(string cacheDirectory)
    {
        Directory.CreateDirectory(cacheDirectory);
        var dbPath = Path.Combine(cacheDirectory, "cache.db");

        _conStr = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true
        }.ToString();
    }
    
    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_conStr);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = BuildMetaCommand();
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string BuildMetaCommand()
    {
        StringBuilder sb = new();
        sb.AppendLine(SQL.Meta.GlobalConfig);
        sb.AppendLine(SQL.Meta.TracksCache);
        sb.AppendLine(SQL.Meta.LyricsCache);
        sb.AppendLine(SQL.Meta.PlaylistsCache);
        sb.AppendLine(SQL.Meta.PersonalCache);
        return sb.ToString();
    }
    
    public async Task<Dictionary<ulong, OnlineTrack>> GetOnlineTracksAsync(IReadOnlyList<ulong> trackIds, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<ulong, OnlineTrack>(trackIds.Count);
        if (trackIds.Count == 0) return result;
        
        await using var connection = new SqliteConnection(_conStr);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = SQL.Track.Get;
        var idsJson = JsonSerializer.Serialize(trackIds.ToArray(), NativeModelSerializationContext.Default.UInt64Array);
        var minValidTime = DateTimeOffset.UtcNow.Subtract(TTL.Track).ToUnixTimeSeconds();
        cmd.Parameters.AddWithValue("$idsJson", idsJson);
        cmd.Parameters.AddWithValue("$minTime", minValidTime);
        cmd.Parameters.AddWithValue("$version", Version.TrackCache);
        
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var id = unchecked((ulong)reader.GetInt64(0));
            var payloadBytes = (byte[])reader["Payload"];
            var track = JsonSerializer.Deserialize(payloadBytes, NativeModelSerializationContext.Default.OnlineTrack);
            if (track is not null) result[id] = track;
        }
        return result;
    }
    
    public async Task SaveOnlineTracksAsync(IReadOnlyList<OnlineTrack> tracks, CancellationToken cancellationToken = default)
    {
        if (tracks.Count == 0) return;

        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var connection = new SqliteConnection(_conStr);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using var cmd = connection.CreateCommand();
            cmd.Transaction = (SqliteTransaction)transaction;
            cmd.CommandText = SQL.Track.Upsert;

            var pId = cmd.Parameters.Add("$id", SqliteType.Integer);
            var pPayload = cmd.Parameters.Add("$payload", SqliteType.Blob);
            var pUpdatedAt = cmd.Parameters.Add("$updatedAt", SqliteType.Integer);
            var pVersion = cmd.Parameters.Add("$version", SqliteType.Integer);
            
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            foreach (var track in tracks)
            {
                pId.Value = unchecked((long)track.TrackId);
                pPayload.Value = JsonSerializer.SerializeToUtf8Bytes(track, NativeModelSerializationContext.Default.OnlineTrack);
                pUpdatedAt.Value = now;
                pVersion.Value = Version.TrackCache;
                
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<TrackLyrics?> GetLyricsAsync(ulong trackId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_conStr);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = SQL.Lyrics.Get;
        cmd.Parameters.AddWithValue("$id", unchecked((long)trackId));
        var minValidTime = DateTimeOffset.UtcNow.Subtract(TTL.Lyrics).ToUnixTimeSeconds();
        cmd.Parameters.AddWithValue("$minTime", minValidTime);
        cmd.Parameters.AddWithValue("$version", Version.LyricsCache);
        
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) return null;
        var payloadBytes = (byte[])reader["Payload"];
        return JsonSerializer.Deserialize(payloadBytes, NativeModelSerializationContext.Default.TrackLyrics);
    }

    public async Task SaveLyricsAsync(ulong trackId, TrackLyrics lyrics, CancellationToken cancellationToken = default)
    {
        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(lyrics, NativeModelSerializationContext.Default.TrackLyrics);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var connection = new SqliteConnection(_conStr);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = SQL.Lyrics.Upsert;
            cmd.Parameters.AddWithValue("$id", unchecked((long)trackId));
            cmd.Parameters.AddWithValue("$payload", payloadBytes);
            cmd.Parameters.AddWithValue("$updatedAt", now);
            cmd.Parameters.AddWithValue("$version", Version.LyricsCache);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<Dictionary<ulong, OnlinePlaylist>> GetPlaylistsAsync(IReadOnlyList<ulong> playlistIds, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<ulong, OnlinePlaylist>(playlistIds.Count);
        if (playlistIds.Count == 0) return result;
        
        await using var connection = new SqliteConnection(_conStr);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = SQL.Playlist.Get;
        var idsJson = JsonSerializer.Serialize(playlistIds.ToArray(), NativeModelSerializationContext.Default.UInt64Array);
        var minValidTime = DateTimeOffset.UtcNow.Subtract(TTL.Playlist).ToUnixTimeSeconds();
        cmd.Parameters.AddWithValue("$idsJson", idsJson);
        cmd.Parameters.AddWithValue("$minTime", minValidTime);
        cmd.Parameters.AddWithValue("$version", Version.PlaylistCache);
        
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var id = unchecked((ulong)reader.GetInt64(0));
            var payloadBytes = (byte[])reader["Payload"];
            var track = JsonSerializer.Deserialize(payloadBytes, NativeModelSerializationContext.Default.OnlinePlaylist);
            if (track is not null) result[id] = track;
        }
        return result;
    }

    public async Task SavePlaylistsAsync(IReadOnlyList<OnlinePlaylist> playlists, CancellationToken cancellationToken = default)
    {
        if (playlists.Count == 0) return;

        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var connection = new SqliteConnection(_conStr);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using var cmd = connection.CreateCommand();
            cmd.Transaction = (SqliteTransaction)transaction;
            cmd.CommandText = SQL.Playlist.Upsert;

            var pId = cmd.Parameters.Add("$id", SqliteType.Integer);
            var pPayload = cmd.Parameters.Add("$payload", SqliteType.Blob);
            var pUpdatedAt = cmd.Parameters.Add("$updatedAt", SqliteType.Integer);
            var pVersion = cmd.Parameters.Add("$version", SqliteType.Integer);
            
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            foreach (var playlist in playlists)
            {
                pId.Value = unchecked((long)playlist.PlaylistId);
                pPayload.Value = JsonSerializer.SerializeToUtf8Bytes(playlist, NativeModelSerializationContext.Default.OnlinePlaylist);
                pUpdatedAt.Value = now;
                pVersion.Value = Version.PlaylistCache;
                
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private const string UserProfileKey = "Profile";
    
    public async Task<UserProfile?> GetPersonalProfileAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_conStr);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = SQL.Personal.Get;
        cmd.Parameters.AddWithValue("$id", UserProfileKey);
        var minValidTime = DateTimeOffset.UtcNow.Subtract(TTL.Personal.Profile).ToUnixTimeSeconds();
        cmd.Parameters.AddWithValue("$minTime", minValidTime);
        cmd.Parameters.AddWithValue("$version", Version.Personal.ProfileCache);
        
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) return null;
        var payloadBytes = (byte[])reader["Payload"];
        return JsonSerializer.Deserialize(payloadBytes, NativeModelSerializationContext.Default.UserProfile);
    }

    public async Task SavePersonalProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var connection = new SqliteConnection(_conStr);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = SQL.Personal.Upsert;
            cmd.Parameters.AddWithValue("$id", UserProfileKey);
            cmd.Parameters.AddWithValue("$payload", JsonSerializer.SerializeToUtf8Bytes(profile, NativeModelSerializationContext.Default.UserProfile));
            cmd.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$version", Version.Personal.ProfileCache);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private const string PersonalPlaylistKey = "Playlists";
    
    public async IAsyncEnumerable<OnlinePlaylist> GetPersonalPlaylistsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_conStr);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        await using var idCmd = connection.CreateCommand();
        idCmd.CommandText = SQL.Personal.Get;
        idCmd.Parameters.AddWithValue("$id", PersonalPlaylistKey);
        var minValidTime = DateTimeOffset.UtcNow.Subtract(TTL.Personal.Playlists).ToUnixTimeSeconds();
        idCmd.Parameters.AddWithValue("$minTime", minValidTime);
        idCmd.Parameters.AddWithValue("$version", Version.Personal.PlaylistsCache);
        
        await using var idReader = await idCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await idReader.ReadAsync(cancellationToken).ConfigureAwait(false)) yield break;
        var payloadBytes = (byte[])idReader["Payload"];
        var ids = JsonSerializer.Deserialize(payloadBytes, NativeModelSerializationContext.Default.UInt64Array);
        if (ids is null || ids.Length == 0) yield break;
        
        await using var plCmd = connection.CreateCommand();
        plCmd.CommandText = SQL.Playlist.GetBrief;
        var idsJson = JsonSerializer.Serialize(ids, NativeModelSerializationContext.Default.UInt64Array);
        plCmd.Parameters.AddWithValue("$idsJson", idsJson);
        minValidTime = DateTimeOffset.UtcNow.Subtract(TTL.Playlist).ToUnixTimeSeconds();
        plCmd.Parameters.AddWithValue("$minTime", minValidTime);
        plCmd.Parameters.AddWithValue("$version", Version.PlaylistCache);
        
        var map = new Dictionary<ulong, OnlinePlaylist>(ids.Length);
        await using var plReader = await plCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await plReader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var id = unchecked((ulong)plReader.GetInt64(0));
            var plPayloadBytes = (byte[])plReader["Payload"];
            var playlist = JsonSerializer.Deserialize(plPayloadBytes, NativeModelSerializationContext.Default.OnlinePlaylist);
            if (playlist is not null) map[id] = playlist;
        }
        
        foreach (var id in ids)
        {
            if (map.TryGetValue(id, out var playlist)) yield return playlist;
            else yield break;
        }
    }

    public async Task SavePersonalPlaylistsAsync(IReadOnlyList<OnlinePlaylist> playlists, CancellationToken cancellationToken = default)
    {
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var connection = new SqliteConnection(_conStr);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using var idCmd = connection.CreateCommand();
            idCmd.Transaction = (SqliteTransaction)transaction;
            idCmd.CommandText = SQL.Personal.Upsert;

            idCmd.Parameters.AddWithValue("$id", PersonalPlaylistKey);
            var ids = playlists.Select(p => p.PlaylistId).ToArray();
            var idsJson = JsonSerializer.SerializeToUtf8Bytes(ids, NativeModelSerializationContext.Default.UInt64Array);
            idCmd.Parameters.AddWithValue("$payload", idsJson);
            idCmd.Parameters.AddWithValue("$updatedAt", now);
            idCmd.Parameters.AddWithValue("$version", Version.Personal.PlaylistsCache);
            
            await idCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            
            var plCmd = connection.CreateCommand();
            plCmd.Transaction = (SqliteTransaction)transaction;
            plCmd.CommandText = SQL.Playlist.UpsertBrief;
            var pId = plCmd.Parameters.Add("$id", SqliteType.Integer);
            var pPayload = plCmd.Parameters.Add("$payload", SqliteType.Blob);
            var pUpdatedAt = plCmd.Parameters.Add("$updatedAt", SqliteType.Integer);
            var pVersion = plCmd.Parameters.Add("$version", SqliteType.Integer);
            
            foreach (var playlist in playlists)
            {
                pId.Value = unchecked((long)playlist.PlaylistId);
                pPayload.Value = JsonSerializer.SerializeToUtf8Bytes(playlist, NativeModelSerializationContext.Default.OnlinePlaylist);
                pUpdatedAt.Value = now;
                pVersion.Value = Version.PlaylistCache;

                await plCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}