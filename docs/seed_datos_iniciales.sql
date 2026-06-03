-- ═══════════════════════════════════════════════════════════════════════
-- SCRIPT DE DATOS INICIALES – Sis_ERP
-- Orden correcto respetando las FK:
--   pais → departamento → moneda → empresa
--   rol  → permiso → rol_permiso
--   departamento_empresa
--   usuario
-- ═══════════════════════════════════════════════════════════════════════

USE sis_erp;

-- ⚠️  Desactivar safe update mode para permitir UPDATE/DELETE sin PK en WHERE
--     (Error 1175 de MySQL Workbench)
SET SQL_SAFE_UPDATES = 0;

-- ── 1. PAÍS ──────────────────────────────────────────────────────────
INSERT INTO pais (id, nombre, codigo_iso, activo)
VALUES (1, 'Perú', 'PE', 1)
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);

-- ── 2. DEPARTAMENTO (geográfico) ─────────────────────────────────────
INSERT INTO departamento (id, pais_id, nombre, activo)
VALUES (1, 1, 'Lima', 1)
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);

-- ── 3. MONEDA ─────────────────────────────────────────────────────────
INSERT INTO moneda (id, nombre, codigo, simbolo, activo)
VALUES (1, 'Sol Peruano', 'PEN', 'S/.', 1)
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);

-- ── 4. EMPRESA ────────────────────────────────────────────────────────
INSERT INTO empresa (id, razon_social, nombre_comercial, ruc, telefono, correo,
                     sitio_web, direccion, ciudad,
                     departamento_id, pais_id, codigo_postal,
                     moneda_id, zona_horaria, idioma,
                     activo, creado_en, actualizado_en)
VALUES (1,
        'HeveLab S.A.C.',
        'HeveLab',
        '20123456789',
        '01-234-5678',
        'contacto@hevelab.com',
        'https://hevelab.com',
        'Av. Principal 123',
        'Lima',
        1,       -- departamento_id → Lima
        1,       -- pais_id → Perú
        'L27',
        1,       -- moneda_id → PEN
        'America/Lima',
        1,
        1,
        NOW(),
        NOW())
ON DUPLICATE KEY UPDATE nombre_comercial = VALUES(nombre_comercial);

-- ── 5. ROL ────────────────────────────────────────────────────────────
INSERT INTO rol (id, nombre, descripcion, activo)
VALUES
  (1, 'Administrador', 'Acceso completo al sistema',   1),
  (2, 'Vendedor',      'Gestión de ventas y clientes',  1),
  (3, 'Contador',      'Módulo contable y financiero',  1)
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);

-- ── 6. PERMISOS ───────────────────────────────────────────────────────
INSERT INTO permiso (id, nombre, codigo, descripcion, activo)
VALUES
  (1,  'Ver Dashboard',         'dashboard.ver',        'Acceso al panel principal',               1),
  (2,  'Gestionar Ventas',      'ventas.gestionar',     'Crear y editar facturas de venta',        1),
  (3,  'Gestionar Compras',     'compras.gestionar',    'Registrar órdenes de compra',             1),
  (4,  'Gestionar Inventario',  'inventario.gestionar', 'Control de stock y almacenes',            1),
  (5,  'Gestionar Clientes',    'clientes.gestionar',   'CRM y directorio de clientes',            1),
  (6,  'Ver Reportes',          'reportes.ver',         'Acceso a reportes y dashboards',          1),
  (7,  'Gestionar Usuarios',    'usuarios.gestionar',   'Administrar usuarios del sistema',        1),
  (8,  'Configurar Sistema',    'sistema.configurar',   'Ajustes generales de la empresa',         1),
  (9,  'Gestionar Contabilidad','contabilidad.gestionar','Módulo contable',                        1),
  (10, 'Gestionar RRHH',        'rrhh.gestionar',       'Recursos humanos y nómina',               1)
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);

-- ── 7. ROL_PERMISO (relación M:N) ────────────────────────────────────
-- Administrador: todos los permisos
INSERT INTO rol_permiso (id, rol_id, permiso_id)
VALUES
  (1, 1, 1),(2, 1, 2),(3, 1, 3),(4, 1, 4),(5, 1, 5),
  (6, 1, 6),(7, 1, 7),(8, 1, 8),(9, 1, 9),(10,1,10)
ON DUPLICATE KEY UPDATE rol_id = VALUES(rol_id);

-- Vendedor: dashboard + ventas + clientes + reportes
INSERT INTO rol_permiso (id, rol_id, permiso_id)
VALUES (11, 2, 1),(12, 2, 2),(13, 2, 5),(14, 2, 6)
ON DUPLICATE KEY UPDATE rol_id = VALUES(rol_id);

-- Contador: dashboard + compras + contabilidad + reportes
INSERT INTO rol_permiso (id, rol_id, permiso_id)
VALUES (15, 3, 1),(16, 3, 3),(17, 3, 6),(18, 3, 9)
ON DUPLICATE KEY UPDATE rol_id = VALUES(rol_id);

-- ── 8. DEPARTAMENTO_EMPRESA (áreas internas) ─────────────────────────
INSERT INTO departamento_empresa (id, empresa_id, nombre, activo)
VALUES
  (1, 1, 'Administración', 1),
  (2, 1, 'Ventas',         1),
  (3, 1, 'Contabilidad',   1),
  (4, 1, 'Almacén',        1),
  (5, 1, 'Sistemas',       1)
ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);

-- ── 9. USUARIO ────────────────────────────────────────────────────────
-- ⚠️  La contraseña de cada usuario es la indicada en el comentario.
--     Hash generado con BCrypt work factor 11.
--
--  admin123     → Administrador principal
--  vendedor123  → Usuario de ventas
--  contador123  → Usuario contable

INSERT INTO usuario (empresa_id, rol_id, departamento_empresa_id,
                     nombre, apellido, nombre_usuario, correo,
                     contrasena, telefono, idioma, zona_horaria,
                     activo, creado_en, actualizado_en)
VALUES
  -- admin / contraseña: admin123
  (1, 1, 1,
   'Administrador', 'Sistema', 'admin', 'admin@hevelab.com',
   '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
   '999000001', 1, 'America/Lima', 1, NOW(), NOW()),

  -- vendedor / contraseña: vendedor123
  (1, 2, 2,
   'Juan', 'Pérez', 'jperez', 'jperez@hevelab.com',
   '$2a$11$UbqU.cGlWKCxSDFMg7bfUOu9XMXxxBe9R5xU5v2LMqVXBlfPwMz2y',
   '999000002', 1, 'America/Lima', 1, NOW(), NOW()),

  -- contador / contraseña: contador123
  (1, 3, 3,
   'María', 'García', 'mgarcia', 'mgarcia@hevelab.com',
   '$2a$11$ov5b.7tBg1vS9P6w/9MwcO9pPOPaJF0LFdJSE0EMSTpZrfJi4ORnO',
   '999000003', 1, 'America/Lima', 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE correo = VALUES(correo);

-- ── VERIFICACIÓN ───────────────────────────────────────────────────────────────
SELECT
    u.id,
    u.nombre_usuario,
    e.nombre_comercial  AS empresa,
    r.nombre            AS rol,
    de.nombre           AS departamento,
    u.activo
FROM usuario u
JOIN empresa            e  ON e.id  = u.empresa_id
JOIN rol                r  ON r.id  = u.rol_id
JOIN departamento_empresa de ON de.id = u.departamento_empresa_id
ORDER BY u.id;

-- Restaurar modo seguro
SET SQL_SAFE_UPDATES = 1;
