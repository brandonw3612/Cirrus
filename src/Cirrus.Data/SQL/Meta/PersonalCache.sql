create table if not exists PersonalCache
(
    Id       text primary key,
    Payload   blob    not null,
    UpdatedAt integer not null,
    Version   integer not null
);

create table if not exists PlaylistsBriefCache
(
    Id        integer primary key,
    Payload   blob    not null,
    UpdatedAt integer not null,
    Version   integer not null
);

create index if not exists Index_PlaylistsBriefCache_UpdatedAt
    on PlaylistsBriefCache (UpdatedAt);