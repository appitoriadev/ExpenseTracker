-- ExpenseTracker.Infrastructure/Data/Schema.sql
-- PostgreSQL schema for Expense Tracker application
-- CREATE DATABASE ExpenseTracker;
CREATE SCHEMA IF NOT EXISTS dbo;

-- Create categories table
CREATE TABLE IF NOT EXISTS dbo.categories (
  id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  category_name VARCHAR(255) NOT NULL UNIQUE,
  created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Create expenses table
CREATE TABLE IF NOT EXISTS dbo.expenses (
  id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  amount NUMERIC(18, 2) NOT NULL CHECK (amount > 0),
  category_id INT NOT NULL,
  expense_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
  created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_categories FOREIGN KEY (category_id) REFERENCES dbo.categories (id)
);

-- Create index on expense_date for faster queries
CREATE INDEX IF NOT EXISTS idx_expenses_date ON dbo.expenses (expense_date DESC);

-- Create index on category_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_expenses_category ON dbo.expenses (category_id);

-- Create users table (for authentication)
CREATE TABLE IF NOT EXISTS dbo.users (
  id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  username VARCHAR(255) UNIQUE NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  firstname VARCHAR(255) NOT NULL,
  lastname VARCHAR(255) NOT NULL,
  created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
  email VARCHAR(255) UNIQUE,
  refresh_token VARCHAR(512),
  refresh_token_expiry TIMESTAMPTZ
);

-- Create index on username for faster lookups
CREATE INDEX IF NOT EXISTS idx_users_username ON dbo.users (username);

-- Create junction table for user-expense relationships
CREATE TABLE IF NOT EXISTS dbo.user_expenses (
  id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  expense_id INT NOT NULL,
  user_id INT NOT NULL,
  created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES dbo.users (id) ON DELETE CASCADE,
  CONSTRAINT fk_expense FOREIGN KEY (expense_id) REFERENCES dbo.expenses (id) ON DELETE CASCADE,
  CONSTRAINT uq_user_expense UNIQUE (user_id, expense_id)
);

-- Create indexes for faster lookups on junction table
CREATE INDEX IF NOT EXISTS idx_userexpenses_user ON dbo.user_expenses (user_id);
CREATE INDEX IF NOT EXISTS idx_userexpenses_expense ON dbo.user_expenses (expense_id);

-- Seed default categories (skip if already present)
INSERT INTO dbo.categories (category_name) VALUES
  ('Food'),
  ('Transport'),
  ('Entertainment'),
  ('Housing'),
  ('Health'),
  ('Shopping'),
  ('Other')
ON CONFLICT (category_name) DO NOTHING;