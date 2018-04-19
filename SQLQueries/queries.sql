--Select * 
--From HawkerCentres.dbo.Address
--Order By latitude, longitude

--Drop table HawkerCentres.dbo.Address


/*Use HawkerCentres
SELECT dbo.Address.latitude as lat, Address.longitude as lon, SQRT(
    POWER(69.1 * (lat - [startlat]), 2) +
    POWER(69.1 * ([startlng] - longitude) * COS(latitude / 57.3), 2)) AS distance
HawkerCentres.dbo.Address HAVING distance < 25 ORDER BY distance;*/

/*Use HawkerCentres
SELECT top 100
(
   3959 *
   acos(cos(radians(37)) * 
   cos(radians(Address.latitude)) * 
   cos(radians(Address.longitude) - 
   radians(-122)) + 
   sin(radians(37)) * 
   sin(radians(Address.latitude )))
) AS distance 
FROM Address
--GROUP BY Address.Id, Address.Name
--HAVING distance < 25 
ORDER BY distance; --LIMIT 0, 20;*/


/*CREATE FUNCTION dbo.DictanceKM(@lat1 FLOAT, @lat2 FLOAT, @lon1 FLOAT, @lon2 FLOAT)
RETURNS FLOAT 
AS
BEGIN

    RETURN ACOS(SIN(PI()*@lat1/180.0)*SIN(PI()*@lat2/180.0)+COS(PI()*@lat1/180.0)*COS(PI()*@lat2/180.0)*COS(PI()*@lon2/180.0-PI()*@lon1/180.0))*6371
END
*/

--Use HawkerCentres
--Select * from DictanceKM(1.00, 100.00, 4.00, 105.00)


/*declare @lat float, @lng float
select @lat = 41.0186, @lng = 28.964701

declare @Location table(Latitude float, Longtitude float, Name nvarchar(50))
insert into @Location(Latitude, Longtitude, Name) values (41.0200500000, 40.5234490000, 'a')
insert into @Location(Latitude, Longtitude, Name) values (41.0185714000, 37.0975924000, 'b')
insert into @Location(Latitude, Longtitude, Name) values (41.0184913000, 34.0373739000, 'c')
insert into @Location(Latitude, Longtitude, Name) values (41.0166667000, 39.5833333000, 'd')
insert into @Location(Latitude, Longtitude, Name) values (41.0166667000, 28.9333333000, 'e')

SELECT ABS(dbo.DictanceKM(@lat, Latitude, @lng, Longtitude)) DistanceKm, * FROM @Location
ORDER BY ABS(dbo.DictanceKM(@lat, Latitude, @lng, Longtitude))
*/

declare @mylat fLOAT, @mylong FLOAT
Select @mylat = 1.297796, @mylong = 103.766223
SELECT ABS(dbo.DictanceKM(@mylat, Address.latitude, @mylong, Address.longitude)) DistanceKm, * from Address
ORDER BY ABS(dbo.DictanceKM(@mylat, Address.latitude, @mylong, Address.longitude)) 