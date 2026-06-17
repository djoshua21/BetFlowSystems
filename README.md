# BetFlow Systems

## Overview

**BetFlow Systems** is an **admin-only backend management tool** built with **ASP.NET MVC** and **Entity Framework Core**, designed to model a structured betting environment with strong domain rules, relational integrity, and maintainable architecture.

The system enables administrators to manage users, user-linked betting accounts, bets, bet types, and the transaction records generated through betting activity. It is built around a dedicated service layer that enforces **business rules and validation logic**, while **EF Core Fluent API** is used to define entity relationships and apply stricter database constraints.

Unlike a basic CRUD application, BetFlow Systems is designed to reflect a more realistic operational workflow where a single user can own **multiple betting accounts**, bets are placed against specific accounts, and transactions are only created when bets are made. This helps ensure better traceability, consistency, and control over account and betting data.

## Key Features

- **Admin-only access control** using authentication and authorization
- **ASP.NET MVC** architecture for clear separation of concerns
- **Entity Framework Core** for database interaction
- **Fluent API** configuration for stronger relationship mapping and constraints
- **Users can own multiple accounts**, allowing flexible wallet management
- **Business and validation rules** handled through the service layer
- **Transactions are generated only when bets are placed**
- Structured relational model covering users, accounts, bets, bet types, transactions, and admins

## Core Domain Entities

- **Admins**  
  Authenticated system users responsible for managing and maintaining the platform.

- **Users**  
  Customer profiles stored in the system.

- **Accounts**  
  Wallet-like betting accounts linked to users. A single user can have multiple accounts.

- **Bets**  
  Betting records linked to both an account and a bet type.

- **Transactions**  
  Financial records created when bets are made.

- **BetTypes**  
  Event and betting category definitions used to describe bet details.

## Architecture and Design

BetFlow Systems is designed as a backend-focused application that prioritizes:

- **Clean separation of concerns** through the MVC pattern
- **Centralized business logic** in the service layer
- **Relational integrity** through EF Core Fluent API configuration
- **Scalability and maintainability** through structured domain modelling

This project goes beyond simple entity management by implementing rules that reflect a more realistic betting workflow, making it a stronger demonstration of backend design and domain logic.

## Project Goal

This project was developed as a **backend-focused portfolio application** to demonstrate:

- practical use of **ASP.NET MVC**
- relational database design with **EF Core**
- advanced model configuration using **Fluent API**
- implementation of **business rules and validation**
- structured handling of betting and account workflows

## Clarification Questions

To make this README even more accurate and polished, the following details can still be clarified:

1. Do admins create and manage **BetTypes** manually before bets are placed?
2. Can admins create bets on behalf of users, or do they only manage existing bets?
3. Do accounts support statuses such as **Active**, **Suspended**, or **Closed**?
4. Is there validation to prevent bets from being placed when an account has insufficient balance?
5. Are transactions created only for bet placement, or also for settlements and winnings?
6. Are bet results updated manually by admins?

## Possible Future Improvements

- Add a **System Architecture** section
- Add a **Database Diagram / ERD**
- Add **Setup Instructions**
- Add **Tech Stack** badges
- Add **Business Rules** documentation
- Add **Screenshots** of the admin interface
