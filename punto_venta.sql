-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: localhost    Database: punto_venta
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `clientes`
--

DROP TABLE IF EXISTS `clientes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientes` (
  `id_cliente` int NOT NULL AUTO_INCREMENT,
  `nombres` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `apellidos` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nit` varchar(12) COLLATE utf8mb4_unicode_ci NOT NULL,
  `genero` bit(1) NOT NULL,
  `telefono` varchar(25) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `correo_electronico` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `fecha_ingreso` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_cliente`),
  UNIQUE KEY `uq_cliente_nit` (`nit`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clientes`
--

LOCK TABLES `clientes` WRITE;
/*!40000 ALTER TABLE `clientes` DISABLE KEYS */;
INSERT INTO `clientes` VALUES (1,'Juan Carlos','Pérez López','1234567-8',_binary '','55551111','juan.perez@correo.com','2026-10-06 21:12:01'),(2,'María Fernanda','Gómez Ruiz','7654321-0',_binary '\0','55552222','maria.gomez@correo.com','2026-10-06 21:12:01'),(3,'Consumidor','Final','CF',_binary '',NULL,NULL,'2026-10-06 21:12:01');
/*!40000 ALTER TABLE `clientes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `compras`
--

DROP TABLE IF EXISTS `compras`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `compras` (
  `id_compra` int NOT NULL AUTO_INCREMENT,
  `no_orden_compra` int NOT NULL,
  `id_proveedor` int NOT NULL,
  `fecha_orden` date NOT NULL,
  `fecha_ingreso` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_compra`),
  KEY `fk_compra_proveedor` (`id_proveedor`),
  CONSTRAINT `fk_compra_proveedor` FOREIGN KEY (`id_proveedor`) REFERENCES `proveedores` (`id_proveedor`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `compras`
--

LOCK TABLES `compras` WRITE;
/*!40000 ALTER TABLE `compras` DISABLE KEYS */;
INSERT INTO `compras` VALUES (1,1001,1,'2026-09-01','2026-10-06 21:12:01'),(2,1002,2,'2026-09-10','2026-10-06 21:12:01');
/*!40000 ALTER TABLE `compras` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `compras_detalle`
--

DROP TABLE IF EXISTS `compras_detalle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `compras_detalle` (
  `id_compra_detalle` bigint NOT NULL AUTO_INCREMENT,
  `id_compra` int NOT NULL,
  `id_producto` int NOT NULL,
  `cantidad` int NOT NULL,
  `precio_costo_unitario` decimal(8,2) NOT NULL,
  PRIMARY KEY (`id_compra_detalle`),
  KEY `fk_cd_compra` (`id_compra`),
  KEY `fk_cd_producto` (`id_producto`),
  CONSTRAINT `fk_cd_compra` FOREIGN KEY (`id_compra`) REFERENCES `compras` (`id_compra`),
  CONSTRAINT `fk_cd_producto` FOREIGN KEY (`id_producto`) REFERENCES `productos` (`id_producto`),
  CONSTRAINT `ck_cd_valores` CHECK (((`cantidad` > 0) and (`precio_costo_unitario` > 0)))
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `compras_detalle`
--

LOCK TABLES `compras_detalle` WRITE;
/*!40000 ALTER TABLE `compras_detalle` DISABLE KEYS */;
INSERT INTO `compras_detalle` VALUES (1,1,1,20,800.00),(2,1,3,50,80.00),(3,2,2,10,4000.00),(4,2,4,30,120.00),(5,2,5,5,1200.00);
/*!40000 ALTER TABLE `compras_detalle` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `empleados`
--

DROP TABLE IF EXISTS `empleados`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `empleados` (
  `id_empleado` int NOT NULL AUTO_INCREMENT,
  `nombres` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `apellidos` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `direccion` varchar(80) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `telefono` varchar(25) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `dpi` varchar(15) COLLATE utf8mb4_unicode_ci NOT NULL,
  `genero` bit(1) NOT NULL,
  `fecha_nacimiento` date NOT NULL,
  `id_puesto` smallint NOT NULL,
  `fecha_inicio_labores` date NOT NULL,
  `fecha_ingreso` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_empleado`),
  UNIQUE KEY `uq_empleado_dpi` (`dpi`),
  KEY `fk_empleado_puesto` (`id_puesto`),
  CONSTRAINT `fk_empleado_puesto` FOREIGN KEY (`id_puesto`) REFERENCES `puestos` (`id_puesto`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `empleados`
--

LOCK TABLES `empleados` WRITE;
/*!40000 ALTER TABLE `empleados` DISABLE KEYS */;
INSERT INTO `empleados` VALUES (1,'Ana Lucía','Castillo Díaz','Zona 1, Guatemala','55553333','2456789010101',_binary '\0','1995-03-15',2,'2022-01-10','2026-10-06 21:12:01'),(2,'Carlos Eduardo','Ramírez Soto','Zona 10, Guatemala','55554444','2987654320101',_binary '','1990-07-22',1,'2020-06-01','2026-10-06 21:12:01'),(3,'Pedro José','Hernández Cruz','Mixco, Guatemala','55555555','3012345670101',_binary '','1988-11-05',3,'2021-03-15','2026-10-06 21:12:01');
/*!40000 ALTER TABLE `empleados` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `marcas`
--

DROP TABLE IF EXISTS `marcas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `marcas` (
  `id_marca` smallint NOT NULL AUTO_INCREMENT,
  `marca` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id_marca`),
  UNIQUE KEY `uq_marca` (`marca`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `marcas`
--

LOCK TABLES `marcas` WRITE;
/*!40000 ALTER TABLE `marcas` DISABLE KEYS */;
INSERT INTO `marcas` VALUES (2,'HP'),(3,'Logitech'),(1,'Samsung');
/*!40000 ALTER TABLE `marcas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `menu`
--

DROP TABLE IF EXISTS `menu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `menu` (
  `id_menu` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ruta` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `id_padre` int DEFAULT NULL,
  `orden` int NOT NULL DEFAULT '0',
  `activo` bit(1) NOT NULL DEFAULT b'1',
  PRIMARY KEY (`id_menu`),
  KEY `fk_menu_padre` (`id_padre`),
  CONSTRAINT `fk_menu_padre` FOREIGN KEY (`id_padre`) REFERENCES `menu` (`id_menu`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `menu`
--

LOCK TABLES `menu` WRITE;
/*!40000 ALTER TABLE `menu` DISABLE KEYS */;
INSERT INTO `menu` VALUES (1,'Productos','/productos',NULL,1,_binary ''),(2,'Marcas ','/marcas',1,1,_binary ''),(3,'Ventas','/ventas',NULL,2,_binary ''),(4,'Clientes','/clientes',3,1,_binary ''),(5,'Empleados','/empleados',3,2,_binary ''),(6,'Puestos','/puestos',5,1,_binary ''),(7,'Compras','/compras',NULL,3,_binary ''),(8,'Proveedores','/proveedores',7,1,_binary ''),(9,'Reportes','/reportes',NULL,4,_binary '');
/*!40000 ALTER TABLE `menu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `productos`
--

DROP TABLE IF EXISTS `productos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `productos` (
  `id_producto` int NOT NULL AUTO_INCREMENT,
  `producto` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `id_marca` smallint NOT NULL,
  `descripcion` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `imagen` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `precio_costo` decimal(8,2) NOT NULL DEFAULT '0.00',
  `precio_venta` decimal(8,2) NOT NULL DEFAULT '0.00',
  `existencia` int NOT NULL DEFAULT '0',
  `fecha_ingreso` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_producto`),
  KEY `fk_producto_marca` (`id_marca`),
  CONSTRAINT `fk_producto_marca` FOREIGN KEY (`id_marca`) REFERENCES `marcas` (`id_marca`),
  CONSTRAINT `ck_producto_existencia` CHECK ((`existencia` >= 0)),
  CONSTRAINT `ck_producto_precios` CHECK (((`precio_costo` >= 0) and (`precio_venta` >= 0)))
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `productos`
--

LOCK TABLES `productos` WRITE;
/*!40000 ALTER TABLE `productos` DISABLE KEYS */;
INSERT INTO `productos` VALUES (1,'Monitor Samsung 24\"',1,'Monitor LED 24 pulgadas Full HD','/images/productos/5c8212d.jpg',800.00,1000.00,18,'2026-10-06 21:12:01'),(2,'Laptop HP 15',2,'Laptop 15.6\" 8GB RAM 256GB SSD','/images/productos/64b1e89.jpg',4000.00,5000.00,9,'2026-10-06 21:12:01'),(3,'Mouse Logitech M185',3,'Mouse inalámbrico','/images/productos/d2ab0fb.webp',80.00,100.00,45,'2026-10-06 21:12:01'),(4,'Teclado Logitech K120',3,'Teclado USB en español','/images/productos/126488e.png',120.00,150.00,27,'2026-10-06 21:12:01'),(5,'Impresora HP DeskJet',2,'Impresora multifuncional','/images/productos/b1478b5.png',1200.00,1500.00,5,'2026-10-06 21:12:01'),(6,'Monitor Samsung 27\"',1,'Monitor LED 27 pulgadas','/images/productos/de6bd97.jpg',1200.00,1500.00,0,'2026-10-06 21:12:01'),(7,'Monitor Hp 24\"',2,'Monitor LED','/images/productos/cdf0fad.png',800.00,1000.00,25,'2026-10-14 00:00:00');
/*!40000 ALTER TABLE `productos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `proveedores`
--

DROP TABLE IF EXISTS `proveedores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `proveedores` (
  `id_proveedor` int NOT NULL AUTO_INCREMENT,
  `proveedor` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nit` varchar(12) COLLATE utf8mb4_unicode_ci NOT NULL,
  `direccion` varchar(80) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `telefono` varchar(25) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id_proveedor`),
  UNIQUE KEY `uq_proveedor_nit` (`nit`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `proveedores`
--

LOCK TABLES `proveedores` WRITE;
/*!40000 ALTER TABLE `proveedores` DISABLE KEYS */;
INSERT INTO `proveedores` VALUES (1,'Distribuidora Tecnológica, S.A.','1111111-2','Zona 4, Guatemala','22221111'),(2,'Importaciones HP Guatemala','2222222-2','Zona 9, Guatemala','22222222'),(3,'Logitech Centroamérica','3333333-3','Zona 12, Guatemala','22223333'),(5,'Distribuidora Tecnológica,','1111111-5','San Miguel petapa','00000000');
/*!40000 ALTER TABLE `proveedores` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `puestos`
--

DROP TABLE IF EXISTS `puestos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `puestos` (
  `id_puesto` smallint NOT NULL AUTO_INCREMENT,
  `puesto` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id_puesto`),
  UNIQUE KEY `uq_puesto` (`puesto`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `puestos`
--

LOCK TABLES `puestos` WRITE;
/*!40000 ALTER TABLE `puestos` DISABLE KEYS */;
INSERT INTO `puestos` VALUES (1,'Administrador'),(3,'Bodeguero'),(2,'Vendedor');
/*!40000 ALTER TABLE `puestos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `id_usuario` int NOT NULL AUTO_INCREMENT,
  `usuario` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL,
  `password_hash` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `activo` bit(1) NOT NULL DEFAULT b'1',
  `id_empleado` int DEFAULT NULL,
  `fecha_ingreso` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_usuario`),
  UNIQUE KEY `uq_usuario` (`usuario`),
  KEY `fk_usuario_empleado` (`id_empleado`),
  CONSTRAINT `fk_usuario_empleado` FOREIGN KEY (`id_empleado`) REFERENCES `empleados` (`id_empleado`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'admin','$2a$11$0iNZeUCA8.DYUuobqMAFBOV5dBd/fJZ0RmbFgBqqRB9MlAfvu.pUG',_binary '',NULL,'2026-10-06 23:22:04');
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ventas`
--

DROP TABLE IF EXISTS `ventas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ventas` (
  `id_venta` int NOT NULL AUTO_INCREMENT,
  `no_factura` int NOT NULL,
  `serie` char(1) COLLATE utf8mb4_unicode_ci NOT NULL,
  `fecha_factura` date NOT NULL,
  `id_cliente` int NOT NULL,
  `id_empleado` int NOT NULL,
  `fecha_ingreso` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_venta`),
  UNIQUE KEY `uq_factura` (`serie`,`no_factura`),
  KEY `fk_venta_cliente` (`id_cliente`),
  KEY `fk_venta_empleado` (`id_empleado`),
  CONSTRAINT `fk_venta_cliente` FOREIGN KEY (`id_cliente`) REFERENCES `clientes` (`id_cliente`),
  CONSTRAINT `fk_venta_empleado` FOREIGN KEY (`id_empleado`) REFERENCES `empleados` (`id_empleado`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ventas`
--

LOCK TABLES `ventas` WRITE;
/*!40000 ALTER TABLE `ventas` DISABLE KEYS */;
INSERT INTO `ventas` VALUES (1,1,'A','2026-09-15',1,1,'2026-10-06 21:12:01'),(2,2,'A','2026-09-20',2,1,'2026-10-06 21:12:01');
/*!40000 ALTER TABLE `ventas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ventas_detalle`
--

DROP TABLE IF EXISTS `ventas_detalle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ventas_detalle` (
  `id_venta_detalle` bigint NOT NULL AUTO_INCREMENT,
  `id_venta` int NOT NULL,
  `id_producto` int NOT NULL,
  `cantidad` int NOT NULL,
  `precio_unitario` decimal(8,2) NOT NULL,
  PRIMARY KEY (`id_venta_detalle`),
  KEY `fk_vd_venta` (`id_venta`),
  KEY `fk_vd_producto` (`id_producto`),
  CONSTRAINT `fk_vd_producto` FOREIGN KEY (`id_producto`) REFERENCES `productos` (`id_producto`),
  CONSTRAINT `fk_vd_venta` FOREIGN KEY (`id_venta`) REFERENCES `ventas` (`id_venta`),
  CONSTRAINT `ck_vd_valores` CHECK (((`cantidad` > 0) and (`precio_unitario` > 0)))
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ventas_detalle`
--

LOCK TABLES `ventas_detalle` WRITE;
/*!40000 ALTER TABLE `ventas_detalle` DISABLE KEYS */;
INSERT INTO `ventas_detalle` VALUES (1,1,1,2,1000.00),(2,1,3,5,100.00),(3,2,2,1,5000.00),(4,2,4,3,150.00);
/*!40000 ALTER TABLE `ventas_detalle` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-14 20:14:38
