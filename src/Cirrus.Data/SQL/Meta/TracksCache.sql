create table if not exists TracksCache
(
    Id        integer primary key,
    Payload   blob    not null,
    UpdatedAt integer not null,
    Version   integer not null
);

create index if not exists Index_TracksCache_UpdatedAt
    on TracksCache (UpdatedAt);