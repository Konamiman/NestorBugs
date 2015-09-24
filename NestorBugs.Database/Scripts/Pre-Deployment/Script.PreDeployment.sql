/*
 Plantilla de script anterior a la implementación							
--------------------------------------------------------------------------------------
 Este archivo contiene instrucciones de SQL que se ejecutarán antes del script de compilación	
 Use sintaxis de SQLCMD para incluir un archivo en el script anterior a la implementación			
 Ejemplo:      :r .\miarchivo.sql								
 Use sintaxis de SQLCMD para hacer referencia a una variable del script anterior a la implementación		
 Ejemplo:      :setvar TableName Mi tabla							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/