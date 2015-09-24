/*
Plantilla de script posterior a la implementación							
--------------------------------------------------------------------------------------
 Este archivo contiene instrucciones de SQL que se anexarán al script de compilación		
 Use sintaxis de SQLCMD para incluir un archivo en el script posterior a la implementación			
 Ejemplo:      :r .\miarchivo.sql								
 Use sintaxis de SQLCMD para hacer referencia a una variable del script posterior a la implementación		
 Ejemplo:      :setvar TableName Mi tabla							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/