select Id, Payload
from TracksCache
where Id in (select value from json_each($idsJson))
  and UpdatedAt >= $minTime
  and Version = $version;