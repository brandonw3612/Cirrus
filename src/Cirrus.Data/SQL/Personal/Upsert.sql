insert into PersonalCache (Id, Payload, UpdatedAt, Version)
values ($id, $payload, $updatedAt, $version)
on conflict (Id) do update set Payload   = excluded.Payload,
                               UpdatedAt = excluded.UpdatedAt,
                               Version   = excluded.Version;