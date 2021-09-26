# HotelReservationManagment

A .NET Core web app - project.

Used working environment and tools: DOT.NET Core 3.0, Entity Frameowork Core (EF Core), Visual Studio 2017/2019, Microsoft SQL Server Management Studio 18

Set up: The app connects to Microsoft SQL Server (.\express) by default. To change it, change the connection string file. To establish connections to your database go to Visual Studio > Package Manager Console and write the command update-database (make sure that the target project is HotelReservation.Data).
When starting the app, make sure that the target project is HotelReservation.Web

Idea of the app: Users are the ones who manage the database. Every user has a role which limits their access to the database. Clients and rooms are linked using the relationship reservation.
