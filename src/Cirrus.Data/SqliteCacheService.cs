using System.Text;
using System.Text.Json;
using Cirrus.Base.Services.Abstract;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Serialization;
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
        return sb.ToString();
    }
    
    public async Task<Dictionary<ulong, OnlineTrack>> GetOnlineTracksAsync(IReadOnlyList<long> trackIds, CancellationToken cancellationToken = default)
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
}