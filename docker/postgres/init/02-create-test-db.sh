#!/bin/bash
set -euo pipefail

# Banco alinhado a appsettings.Testing.json (opcional para testes locais contra compose)
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres <<-EOSQL
    SELECT 'CREATE DATABASE smartcondo_test'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'smartcondo_test')\gexec
EOSQL

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname smartcondo_test <<-EOSQL
    CREATE EXTENSION IF NOT EXISTS vector;
EOSQL
