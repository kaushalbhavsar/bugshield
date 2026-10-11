SELECT 
  SourceIp,
  count(id) as count
FROM ShieldLog  
GROUP BY SourceIp order by count desc