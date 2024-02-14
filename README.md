# Student ID Generator with sms and email notification

## Description
This is a simple student ID generator with sms and email notification. It is a simple application that allows the user to generate, view, and reset student IDs. It also allows the user to send sms and email notifications to the students. The application is built using C# and .NET and uses free API for sending sms notifications and SMTP for sending email notifications to the students.

## Features
Two user roles: Admin and Student
- Admin
  - Approve student registration
  - Approve student Reset ID request
  - Send sms and email notifications to students

- Student
    - Register
    - Request for ID reset
    - View student ID
    - View student profile

## System Requirements

for development:
- Visual Studio 2017 or later
- .NET Framework 4.8 or later
- SQL Server 2022 or later

for deployment:
- Windows 10 21H1 or later
- .NET Framework 4.8 or later
- SQL Server 2022

## Installation

In order to run the application, you need to have the following installed on your machine:
- Visual Studio 2017 or later
- .NET Framework 4.8 or later
- SQL Server 2022 or later

### Database Setup
- Open SQL Server Management Studio (SSMS)
- Connect to your local server
- Right-click on the Databases node and Import Data-tier Application
- Select the .bacpac file from the database folder in the source code
- Follow the import wizard to import the database

### Application Setup
- Open the solution file in Visual Studio
- Update the connection string in the `App.config` file to match your local server
- Build and run the application
- Use the following credentials to login:
  - Admin
    - Username: admin
    - Password: admin
    
  for student, you can register a new student account





