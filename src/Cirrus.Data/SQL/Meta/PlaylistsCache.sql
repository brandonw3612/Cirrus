create table if not exists PlaylistsCache
(
    Id        integer primary key,
    Payload   blob    not null,
    UpdatedAt integer not null,
    Version   integer not null
);

create index if not exists Index_PlaylistsCache_UpdatedAt
    on PlaylistsCache (UpdatedAt);