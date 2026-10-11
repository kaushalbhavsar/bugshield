SELECT 
  attackid, bugdb.title,
  count(ShieldLog.id) as count
FROM ShieldLog, BugDb where date = '2/25/2017' and ShieldLog.AttackId = BugDb.Id 
GROUP BY AttackId, BugDb.Title   order by   count desc   