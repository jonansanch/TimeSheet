# Trabajo diferido

- Unificar la administración del esquema entre migraciones EF y `EnsureTimesheetTablesAsync`, incluyendo serialización de DDL para arranques concurrentes. Detectado al corregir la creación de `AspNetUsers.CodigoPais`; es una condición arquitectónica preexistente que afecta más columnas y requiere una intervención separada.
