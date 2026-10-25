create table if not exists LyricsCache
(
    Id        integer primary key,
    Payload   blob    not null,
    UpdatedAt integer not null,
    Version   integer not null
);

create index if not exists Index_LyricsCache_UpdatedAt
    on LyricsCache (UpdatedAt);