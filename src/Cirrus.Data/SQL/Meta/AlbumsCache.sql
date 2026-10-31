create table if not exists AlbumsCache
(
    Id        integer primary key,
    Payload   blob    not null,
    UpdatedAt integer not null,
    Version   integer not null
);

create index if not exists Index_AlbumsCache_UpdatedAt
    on AlbumsCache (UpdatedAt);