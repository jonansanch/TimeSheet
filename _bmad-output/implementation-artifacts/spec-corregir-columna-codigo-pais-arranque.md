---
title: 'Corregir creación de CodigoPais al iniciar'
type: 'bugfix'
created: '2026-09-24'
status: 'done'
route: 'one-shot'
---

# Corregir creación de CodigoPais al iniciar

## Intent

**Problem:** La aplicación consultaba `AspNetUsers.CodigoPais` durante el sembrado, pero las bases existentes creadas con `EnsureCreated` no recibían automáticamente la migración EF y fallaban al arrancar.

**Approach:** Incorporar la columna nullable al bloque DDL idempotente que el proyecto ejecuta antes del sembrado, conservando la migración para instalaciones gestionadas mediante EF.

## Suggested Review Order

- El inicializador repara la columna antes de consultar o sembrar usuarios.
  [`ApplicationDbContextInitialiser.cs:715`](../../Backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs#L715)

- La comprobación idempotente permite reiniciar bases actualizadas sin repetir el ALTER.
  [`ApplicationDbContextInitialiser.cs:738`](../../Backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs#L738)
