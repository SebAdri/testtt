-- Ejecutar conectado a la base "accountsdb" (creada con 01_create_database.sql).
-- psql: psql -h localhost -p 5432 -U postgres -d accountsdb -f 02_create_table_and_seed.sql

CREATE TABLE IF NOT EXISTS accounts (
    id             UUID PRIMARY KEY,
    account_number VARCHAR(20)  NOT NULL,
    holder_name    VARCHAR(150) NOT NULL,
    balance        NUMERIC(18,2) NOT NULL DEFAULT 0,
    status         VARCHAR(20)  NOT NULL,
    created_at     TIMESTAMP    NOT NULL DEFAULT NOW(),
    CONSTRAINT ix_accounts_account_number UNIQUE (account_number)
);

INSERT INTO accounts (id, account_number, holder_name, balance, status, created_at) VALUES
    ('a1a1a1a1-0001-0001-0001-000000000001', 'ACC-0001', 'Lionel Messi',  15000.50, 'Active',   NOW()),
    ('a1a1a1a1-0002-0002-0002-000000000002', 'ACC-0002', 'Julian Alvarez',    8320.00, 'Active',   NOW()),
    ('a1a1a1a1-0003-0003-0003-000000000003', 'ACC-0003', 'Sebas Caballero',      0.00, 'Inactive', NOW())
ON CONFLICT (account_number) DO NOTHING;

SELECT * FROM accounts;
