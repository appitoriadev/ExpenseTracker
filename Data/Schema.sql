-- ExpenseTracker.Infrastructure/Data/Schema.sql
-- PostgreSQL schema for Expense Tracker application
CREATE SCHEMA IF NOT EXISTS dbo;


-- Create categories table
CREATE TABLE IF NOT EXISTS dbo.categories (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  category_name VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


-- Create index on date for faster queries
CREATE INDEX IF NOT EXISTS idx_categories ON dbo.categories (id DESC);


-- Create expenses table
CREATE TABLE IF NOT EXISTS dbo.expenses (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  title VARCHAR(255) NOT NULL,
  amount NUMERIC(18, 2) NOT NULL CHECK (amount > 0),
  category_id UUID NOT NULL,
  date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_categories FOREIGN KEY (category_id) REFERENCES dbo.categories (id)
);


-- Create index on date for faster queries
CREATE INDEX IF NOT EXISTS idx_expenses_date ON dbo.expenses (date DESC);

-- Create index on category_id for faster group queries
CREATE INDEX IF NOT EXISTS idx_expenses_category ON dbo.expenses (category_id DESC);


-- Create users table (for authentication)
CREATE TABLE IF NOT EXISTS dbo.users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  username VARCHAR(255) UNIQUE NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  firstname VARCHAR(255) UNIQUE NOT NULL,
  lastname VARCHAR(255) UNIQUE NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


-- Create index on username for faster lookups
CREATE INDEX IF NOT EXISTS idx_users_username ON dbo.users (username);


-- Create relational tables
CREATE TABLE IF NOT EXISTS dbo.user_expenses (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  expenses_id UUID NOT NULL,
  user_id UUID NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES dbo.users (id),
  CONSTRAINT fk_expenses FOREIGN KEY (expenses_id) REFERENCES dbo.expenses (id)
);


-- Create indexes on user_expenses_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_userexpenses_ids ON dbo.users (id);

-- Create indexes on expenses_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_expenses_ids ON dbo.users (id);