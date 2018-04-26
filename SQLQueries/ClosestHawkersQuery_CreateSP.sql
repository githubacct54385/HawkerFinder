USE HawkerCentres;  
GO  
CREATE PROCEDURE CLOSEST_HAWKERS
    @Lat FLOAT,   
    @Lng FLOAT   
AS   
    SET NOCOUNT ON;  
    SELECT top 5 ABS(dbo.DictanceKM(@Lat, Address.latitude, @Lng, 
    Address.longitude)) DistanceKm, * from Address
    ORDER BY ABS(dbo.DictanceKM(@Lat, Address.latitude, @Lng, 
    Address.longitude))
GO  