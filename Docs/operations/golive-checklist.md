# Checklist de Go-Live — KPG Timesheet

**Versión:** 1.0  
**Fecha de preparación:** 2026-05-20  
**Responsable técnico:** ___________________  
**Fecha de go-live planificada:** ___________________

---

## Instrucciones

Completar este checklist **antes** de abrir el sistema a los usuarios finales. Cada ítem debe marcarse como:
- `[x]` — Completado y verificado
- `[!]` — Completado con observaciones (documentar abajo)
- `[n/a]` — No aplica en este entorno

---

## Sección 1 — Infraestructura del Servidor

- [x] Servidor Windows disponible (IIS o servicio Windows con Kestrel)
- [x] .NET 10 Runtime instalado en el servidor
- [x] SQL Server instalado y accesible desde el servidor de aplicación
- [x] Certificado SSL/TLS vigente para el dominio (e.g. `https://timesheet.kpg.com`)
- [x] Puerto HTTPS (443) abierto en el firewall interno
- [x] Carpeta de backups creada: `D:\Backups\Timesheet\` (o ruta acordada)
- [x] Carpeta de logs con permisos de escritura para el proceso de aplicación
- [x] Resolución DNS interna apuntando al servidor (`timesheet.kpg.com` → IP del servidor)
- [x] Resolución probada desde al menos una PC de usuario (ping/browser)

**Observaciones:**

---

## Sección 2 — Configuración de la Aplicación

- [x] `appsettings.Production.json` configurado con:
  - [x] `ConnectionStrings.DefaultConnection` apuntando a la BD de producción
  - [x] `Jwt.Key` configurado con valor seguro (≥ 32 caracteres, aleatorio)
  - [x] `Jwt.Issuer` y `Jwt.Audience` configurados correctamente
  - [x] `Smtp.Host` configurado (dejado vacío en go-live)
- [x] Archivo `appsettings.json` NO contiene contraseñas de producción (secrets en Azure App Service Application Settings)
- [x] Las secciones `"Serilog"` en `appsettings.Production.json` están configuradas (solo sink de archivo, sin consola)
- [ ] Aplicación compilada en modo `Release` (`dotnet publish --configuration Release`)
- [ ] Archivos publicados desplegados en el servidor

**Observaciones:** Secrets (ConnectionString, JWT) configurados como Azure App Service Application Settings, no en archivos de configuración. Deploy automático vía GitHub Actions al hacer push a master.

---

## Sección 3 — Base de Datos

- [n/a] SQL Server Agent habilitado (Azure SQL Managed — no aplica)
- [x] Base de datos `TimesheetDb` creada
- [x] Migraciones aplicadas: `dotnet ef database update --project Backend/src/Infrastructure --startup-project Backend/src/Api`
- [x] Verificar tablas creadas (al menos: `AspNetUsers`, `RegistroHoras`, `BitacoraEventos`, `Empleados`, `Clientes`, `Proyectos`)
- [ ] Seed inicial ejecutado (Admin, catálogos base cargados)
- [x] Verificar que el usuario de aplicación tiene permisos sobre `TimesheetDb`

**Observaciones:** BD en Azure SQL (sqltimesheetv1js2026.database.windows.net). Migraciones aplicadas manualmente el 2026-05-24.

---

## Sección 4 — Accesos y Roles

- [ ] Usuario Admin creado: `admin@kpg.com` (o el correo real del administrador KPG)
- [ ] Admin puede hacer login y accede a todas las secciones del menú
- [ ] Supervisor(es) del equipo creados con rol `Supervisor`
- [ ] Gerente(s) creados con rol `Gerente`
- [ ] Todos los empleados activos de KPG creados con rol `Empleado`
- [ ] Contraseñas iniciales comunicadas de forma segura a cada usuario (nunca por correo en texto plano)
- [ ] Los usuarios saben cómo cambiar su contraseña (contactar al Admin)
- [ ] Admin conoce el procedimiento para resetear contraseñas de usuarios

**Lista de usuarios creados:**

| Nombre | Email | Rol | Creado |
|--------|-------|-----|--------|
| | | | [ ] |
| | | | [ ] |
| | | | [ ] |
| | | | [ ] |
| | | | [ ] |

**Observaciones:**

---

## Sección 5 — Backups

- [ ] Job de SQL Server Agent creado: `KPG_Timesheet_DailyBackup` (ver `backup-recovery.md` para el T-SQL)
- [ ] Job programado a las **23:00** horas, frecuencia diaria
- [ ] Backup inicial manual ejecutado correctamente:
  ```sql
  BACKUP DATABASE [TimesheetDb]
  TO DISK = N'D:\Backups\Timesheet\TimesheetDb_PREDEPLOYMENT.bak'
  WITH COMPRESSION, STATS = 10;
  ```
- [ ] Backup pre-deploy verificado con `RESTORE VERIFYONLY`:
  ```sql
  RESTORE VERIFYONLY FROM DISK = N'D:\Backups\Timesheet\TimesheetDb_PREDEPLOYMENT.bak';
  ```
- [ ] Archivo `.bak` pre-deploy conservado y etiquetado como "pre-golive"
- [ ] Espacio en disco suficiente (mínimo 20 GB disponibles en la unidad de backups)

**Observaciones:**

---

## Sección 6 — Monitoreo Básico

- [ ] El responsable técnico sabe la ruta de los logs: `<ruta-app>/logs/kpg-timesheet-YYYYMMDD.log`
- [ ] El responsable técnico puede leer los logs (acceso al servidor o carpeta compartida)
- [ ] Verificar que los logs se están escribiendo al hacer requests de prueba
- [ ] El Job de SQL Server Agent tiene configurada notificación en caso de fallo (operador SQL Server o correo)

**Observaciones:**

---

## Sección 7 — Verificación de la Aplicación

- [ ] Frontend Blazor WASM carga en el navegador (tiempo de carga inicial < 10 segundos en red LAN)
- [ ] Login funciona con las credenciales de Admin
- [ ] Logout funciona correctamente
- [ ] Al navegar a rutas no autorizadas, el sistema redirige al login (no muestra error 500)
- [ ] Los 4 navegadores base funcionan: Chrome ✓, Edge ✓, Firefox ✓
- [ ] Resolución 1280px: navegación y formularios utilizables (puede haber degradación menor)
- [ ] Resolución 1440px: experiencia completa
- [ ] Resolución 1920px: experiencia completa

**Observaciones:**

---

## Sección 8 — Ventana de Reversión

- [ ] El responsable técnico conoce el procedimiento de rollback en `backup-recovery.md`
- [ ] Se ha estimado el tiempo de rollback (RTO ≤ 4 horas según SLA)
- [ ] El backup pre-deploy (`TimesheetDb_PREDEPLOYMENT.bak`) está accesible y verificado
- [ ] Existe un plan de comunicación para notificar a los usuarios en caso de rollback
- [ ] Se ha definido el criterio de decisión: ¿cuándo hacer rollback vs. parchar en caliente?
  - Rollback si: falla de datos, pérdida de acceso generalizada, bug de seguridad crítico
  - Parchar si: falla menor de UX, un rol afectado, comportamiento inesperado no crítico

**Observaciones:**

---

## Sección 9 — Comunicación Pre-Launch

- [ ] Email de anuncio enviado a los empleados con URL, fecha de disponibilidad e instrucciones de primer login
- [ ] Sesión de capacitación breve programada (< 1 hora recomendada por PRD)
- [ ] Canal de reporte de problemas informado a todos los usuarios (teléfono o correo del responsable)
- [ ] Período de hiper care comunicado (primera semana — ver `hypercare-plan.md`)

**Observaciones:**

---

## Firma de Autorización de Go-Live

| Rol | Nombre | Firma | Fecha |
|-----|--------|-------|-------|
| Responsable Técnico | | | |
| Product Owner / Líder KPG | | | |

**¿Defectos críticos abiertos al momento del go-live?**

- [ ] NO — El sistema está listo para producción
- [ ] SÍ — Listar defectos abiertos y decisión de go/no-go:

| # | Descripción | Severidad | Decisión |
|---|-------------|-----------|----------|
| 1 | | | |

---

> Ver también: [`backup-recovery.md`](backup-recovery.md) — procedimiento de recuperación  
> Ver también: [`availability-sla.md`](availability-sla.md) — SLA y proceso de incidentes  
> Ver también: [`hypercare-plan.md`](hypercare-plan.md) — plan de soporte semana 1
