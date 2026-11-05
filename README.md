# SmartStock

SmartStock is an inventory and stock management system built with **Python 3.10**, **Django 5.2.19**, and **SQLite**.

## Features

- Django admin login and application authentication
- Database-backed dashboard with product count, stock valuation, and low-stock alerts
- Product, category, and supplier management
- Atomic stock-in and stock-out operations with insufficient-stock protection
- Search and category/stock-level filters
- Transaction history with movement filters
- Category inventory composition and movement analytics

## Run locally

```powershell
.\.venv\Scripts\python.exe manage.py runserver
```

Open `http://127.0.0.1:8000/`. Create an administrator with:

```powershell
.\.venv\Scripts\python.exe manage.py createsuperuser
```

The database is `db.sqlite3`. All inventory and reporting values are queried from Django models; the dashboard does not use hard-coded data collections.
