select Id, Payload
from AlbumsBriefCache
where Id in (select value from json_each($idsJson))
  and UpdatedAt >= $minTime
  and Version = $version;