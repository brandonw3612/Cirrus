select Payload
from PersonalCache
where Id = $id
  and UpdatedAt >= $minTime
  and Version = $version;