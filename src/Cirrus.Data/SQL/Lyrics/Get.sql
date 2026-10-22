select Payload
from LyricsCache
where Id = $id
  and UpdatedAt >= $minTime;