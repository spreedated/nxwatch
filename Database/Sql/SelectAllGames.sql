SELECT a.name,a.date,a.link,a.nxdate, group_concat(c.name, ";") AS categories FROM games AS a
LEFT JOIN gameCategoriesMapping AS b ON a.id=b.game
LEFT JOIN categories AS c ON b.category=c.id
GROUP BY a.id;