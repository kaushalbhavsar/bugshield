SELECT date,count(id)
	FROM ShieldLog  where Host = 'bugskan.com'
	GROUP BY date order by date desc