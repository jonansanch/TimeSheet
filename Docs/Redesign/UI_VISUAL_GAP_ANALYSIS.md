# KPG Timesheet --- Visual Gap Analysis

## Diagnóstico

La primera iteración fue principalmente un **reskin**: mejoró shell,
colores y superficies, pero conservó demasiada composición heredada. La
siguiente iteración debe priorizar layout refactoring y visual fidelity.

  -----------------------------------------------------------------------
  Vista                   Gap principal           Prioridad
  ----------------------- ----------------------- -----------------------
  Login                   Compactar formulario e  Media
                          integrar mejor branding 

  Inicio                  Falta resumen operativo Alta
                          del mockup              

  Registro                Sigue usando calendario Crítica
                          grande + formulario     
                          heredado                

  Mis Registros           Falta KPI/filter        Crítica
                          composition; N1/N2/N3   
                          genera ruido            

  Dashboard               Exceso de cards         Crítica
                          individuales            

  Aprobaciones            Falta patrón de bandeja Alta
                          de trabajo              

  Organigrama             Sigue siendo lista      Crítica
                          indentada               

  Administración          Falta patrón unificado  Alta
                          de                      
                          toolbar/filtros/tabla   
  -----------------------------------------------------------------------

## Gap global

El AppShell ya está encaminado; no debe consumir la mayor parte de la
segunda iteración. El target exige mayor densidad útil, cards compactas
y jerarquía `título/acción → KPIs/contexto → tarea principal → detalle`.

## Cierre

Cada gap requiere comparación visual en navegador contra la referencia
específica, además de build/tests. Compartir colores o border-radius con
el target no es suficiente.
