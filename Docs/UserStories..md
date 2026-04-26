# Epic: Core Expense Management

## Main User Story

As a budget-conscious user, I want to securely log, view, edit, and delete my daily expenses so that I can accurately track my spending habits over time.

---

## Backend User Stories

### US-B1: User Authentication

**As a** backend system
**I want to** authenticate users via login credentials and issue JWT tokens
**So that** only authorized users can access their expense data.

**Acceptance Criteria:**

- POST `/api/auth/login` accepts username and password
- Returns signed JWT token with 1-hour expiration
- Invalid credentials return 401 Unauthorized
- Token includes user identifier in claims
- CORS allows React frontend origins (localhost:5173, localhost:3000)

---

### US-B2: Create Expense (Backend)

**As a** backend API
**I want to** accept and persist new expense records
**So that** user expenses are stored in the database.

**Acceptance Criteria:**

- POST `/api/expenses` requires [Authorize] header with valid JWT
- Accepts JSON: { title, amount, category, date }
- Validates: title (1-255 chars), amount (>0, ≤999999.99), category (1-100 chars)
- Returns 400 Bad Request for invalid data
- Persists to PostgreSQL with UUID and timestamp
- Returns 201 Created with complete expense object including ID

---

### US-B3: Retrieve All Expenses (Backend)

**As a** backend API
**I want to** retrieve paginated or complete list of user expenses
**So that** frontend can display expense history.

**Acceptance Criteria:**

- GET `/api/expenses` requires [Authorize]
- Returns list of all expenses sorted by date DESC
- Returns 200 OK with JSON array
- Eager loads all expense properties (no lazy loading issues)
- Performance: returns large lists in <500ms

---

### US-B4: Retrieve Single Expense (Backend)

**As a** backend API
**I want to** return a specific expense by ID
**So that** frontend can populate edit forms or show detail views.

**Acceptance Criteria:**

- GET `/api/expenses/{id}` requires [Authorize]
- Returns 200 OK with expense object if found
- Returns 404 Not Found if expense doesn't exist
- ID validation: must be valid UUID format

---

### US-B5: Update Expense (Backend)

**As a** backend API
**I want to** accept updates to existing expense records
**So that** users can correct mistakes or change expense data.

**Acceptance Criteria:**

- PUT `/api/expenses/{id}` requires [Authorize]
- Accepts JSON: { title, amount, category, date }
- Validates same rules as Create (US-B2)
- Returns 404 if expense not found
- Returns 400 for invalid data
- Returns 200 OK with updated expense
- Updates timestamp is preserved; DB record shows update occurred

---

### US-B6: Delete Expense (Backend)

**As a** backend API
**I want to** remove expense records
**So that** users can delete unwanted or accidental entries.

**Acceptance Criteria:**

- DELETE `/api/expenses/{id}` requires [Authorize]
- Returns 404 if expense not found
- Returns 204 No Content on success
- Expense is fully removed from database

---

### US-B7: Input Validation & Error Handling

**As a** backend system
**I want to** validate all inputs and return clear error messages
**So that** frontend can guide users to fix problems.

**Acceptance Criteria:**

- All validation errors return 400 Bad Request
- Error response includes field name and issue (e.g., "amount must be positive")
- Unhandled exceptions return 500 Internal Server Error with generic message
- No sensitive data leaks in error responses
- All async operations have try-catch and log failures

---

### US-B8: Database Persistence

**As a** backend system
**I want to** reliably store and retrieve expenses from PostgreSQL
**So that** data is consistent and available after restarts.

**Acceptance Criteria:**

- Expenses table exists with schema: id (UUID), title, amount, category, date, created_at
- All CRUD operations use ADO.NET with NpgsqlConnection
- Database connection string loaded from appsettings.json
- Numeric precision: amount stored as NUMERIC(18,2)
- Date stored as TIMESTAMP with timezone awareness

---

## Frontend User Stories

### US-F1: Login Page

**As a** user
**I want to** enter credentials on a login screen
**So that** I can authenticate and access my expenses.

**Acceptance Criteria:**

- Login form has username and password fields
- Submit button sends POST to `/api/auth/login`
- On success (200): store JWT in localStorage/sessionStorage, redirect to dashboard
- On failure (401): display "Invalid credentials" error message
- Disable submit button while request is in flight
- Show "Remember me" option (optional: pre-fill username)
- Clear error messages when user starts typing

---

### US-F2: Dashboard / Expenses List

**As a** user
**I want to** see all my expenses in a table or list
**So that** I can review my spending at a glance.

**Acceptance Criteria:**

- Display expenses in chronological order (newest first) or sortable by date/amount
- Table columns: Title, Amount, Category, Date
- Show total expense count or total amount spent
- Load expenses on component mount via GET `/api/expenses`
- Display loading indicator while fetching
- Show error message if fetch fails (with retry button)
- On auth failure (401): redirect to login page

---

### US-F3: Add Expense Form

**As a** user
**I want to** fill out a form to add a new expense
**So that** I can record spending quickly.

**Acceptance Criteria:**

- Form has input fields: Title, Amount, Category (dropdown or text), Date (date picker)
- Submit button labeled "Add Expense" or "Save"
- Validate client-side: title not empty, amount > 0, category not empty
- Show validation errors inline before submission
- On submit: POST to `/api/expenses` with JWT auth header
- On success: show success toast/alert, clear form, refresh expenses list
- On error: show error message (not generic)
- Disable submit while request pending

---

### US-F4: Edit Expense Modal or Form

**As a** user
**I want to** click an expense to edit its details
**So that** I can correct mistakes or update information.

**Acceptance Criteria:**

- Click "Edit" button on expense row → open modal or navigate to edit page
- Pre-populate form with current expense values (GET `/api/expenses/{id}`)
- Edit form same as Create (US-F3) but title: "Edit Expense"
- Submit sends PUT to `/api/expenses/{id}` with updated data
- On success: refresh list, show "Expense updated" toast
- On error: show error message
- Disable submit while request pending

---

### US-F5: Delete Expense Confirmation

**As a** user
**I want to** delete an expense with a confirmation prompt
**So that** I don't accidentally remove entries.

**Acceptance Criteria:**

- Click "Delete" button on expense row
- Show confirmation dialog: "Are you sure you want to delete this expense?"
- If confirmed: DELETE `/api/expenses/{id}` with JWT auth
- On success: remove from list, show "Expense deleted" toast
- On error: show error message
- If cancelled: dismiss dialog, take no action

---

### US-F6: Session & Token Management

**As a** frontend system
**I want to** manage JWT tokens and handle expiration
**So that** users stay logged in and are prompted when session expires.

**Acceptance Criteria:**

- Store JWT in localStorage on successful login
- Include JWT in Authorization header for all API calls
- Detect 401 responses and redirect user to login
- On logout: clear token from storage
- Show "Session expired" message before redirecting on 401
- Refresh token silently before expiration (optional future enhancement)

---

### US-F7: Logout

**As a** user
**I want to** click a logout button
**So that** I can end my session and return to login.

**Acceptance Criteria:**

- Logout button in header/navbar
- On click: clear JWT token from storage
- Redirect to login page
- Show "You have been logged out" message (optional)

---

### US-F8: Filter & Search Expenses (Optional)

**As a** user
**I want to** filter expenses by category or search by title
**So that** I can find specific expenses quickly.

**Acceptance Criteria:**

- Add filter dropdown (Category) above expense list
- Add search box to filter expenses by title (client-side or server-side)
- Update list in real-time as user types/selects
- Show "No expenses found" if filter results are empty
- Clear filters button to reset to full list

---

### US-F9: Responsive Design

**As a** user on mobile or tablet
**I want to** use the app on small screens
**So that** I can manage expenses on the go.

**Acceptance Criteria:**

- Layout adapts to screens <768px, 768px–1024px, >1024px
- Form inputs and buttons are touch-friendly (min 44px height)
- Table converts to card layout on mobile
- Navigation collapses to hamburger menu on mobile
- No horizontal scroll required

---

### US-F10: Error Handling & User Feedback

**As a** user
**I want to** see clear error messages when things go wrong
**So that** I understand what to do next.

**Acceptance Criteria:**

- Network errors show "Connection failed. Please try again."
- Server errors (500) show "Something went wrong. Please contact support."
- Validation errors show specific field-level messages
- Toast/snackbar messages auto-dismiss after 3–5 seconds
- Errors are dismissible by clicking an X or after timeout
- Critical errors (auth failure) don't dismiss automatically

---

## Shared Concerns (Both Frontend & Backend)

### US-S1: CORS Security

**As a** system
**I want to** allow React frontend origins and block others
**So that** endpoints are consumed only by authorized frontends.

**Acceptance Criteria:**

- Backend CORS policy allows `localhost:5173`, `localhost:3000`
- Credentials (cookies) allowed if needed
- Preflight OPTIONS requests return 200
- Invalid origins receive 403 Forbidden

---

### US-S2: API Documentation

**As a** frontend developer
**I want to** see interactive API docs (Swagger/OpenAPI)
**So that** I can test endpoints and understand request/response formats.

**Acceptance Criteria:**

- GET `/swagger` or `/swagger/index.html` shows Swagger UI
- All endpoints documented with descriptions
- Request/response schemas shown
- "Try it out" feature works for testing
- Bearer token field in Swagger for passing JWT

---

## Implementation Priority

**Phase 1 (MVP):**

- US-B1, US-B2, US-B3, US-B4, US-B5, US-B6 (Backend CRUD + Auth)
- US-F1, US-F2, US-F3, US-F5, US-F7 (Frontend Core)
- US-S1, US-S2 (Infrastructure)

**Phase 2 (Enhancement):**

- US-B7, US-B8 (Improved validation & persistence)
- US-F4, US-F6 (Edit & token management)
- US-F9, US-F10 (UX polish)

**Phase 3 (Optional/Future):**

- US-F8 (Search & filtering)
- Advanced reports, recurring expenses, budget alerts
