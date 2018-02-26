--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.5
-- Dumped by pg_dump version 9.6.5

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: postgres; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON DATABASE postgres IS 'default administrative connection database';


--
-- Name: tiger; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA tiger;


ALTER SCHEMA tiger OWNER TO postgres;

--
-- Name: tiger_data; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA tiger_data;


ALTER SCHEMA tiger_data OWNER TO postgres;

--
-- Name: topology; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA topology;


ALTER SCHEMA topology OWNER TO postgres;

--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- Name: fuzzystrmatch; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS fuzzystrmatch WITH SCHEMA public;


--
-- Name: EXTENSION fuzzystrmatch; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION fuzzystrmatch IS 'determine similarities and distance between strings';


--
-- Name: postgis; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS postgis WITH SCHEMA public;


--
-- Name: EXTENSION postgis; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis IS 'PostGIS geometry, geography, and raster spatial types and functions';


--
-- Name: postgis_tiger_geocoder; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS postgis_tiger_geocoder WITH SCHEMA tiger;


--
-- Name: EXTENSION postgis_tiger_geocoder; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis_tiger_geocoder IS 'PostGIS tiger geocoder and reverse geocoder';


--
-- Name: postgis_topology; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS postgis_topology WITH SCHEMA topology;


--
-- Name: EXTENSION postgis_topology; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis_topology IS 'PostGIS topology spatial types and functions';


SET search_path = public, pg_catalog;

--
-- Name: detection_class; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE detection_class AS ENUM (
    'Unknown',
    'Compacts',
    'Sedans',
    'SUVs',
    'Coupes',
    'Muscle',
    'SportsClassics',
    'Sports',
    'Super',
    'Motorcycles',
    'OffRoad',
    'Industrial',
    'Utility',
    'Vans',
    'Cycles',
    'Boats',
    'Helicopters',
    'Planes',
    'Service',
    'Emergency',
    'Military',
    'Commercial',
    'Trains'
);


ALTER TYPE detection_class OWNER TO postgres;

--
-- Name: detection_type; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE detection_type AS ENUM (
    'background',
    'person',
    'car',
    'bicycle'
);


ALTER TYPE detection_type OWNER TO postgres;

--
-- Name: weather; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE weather AS ENUM (
    'Unknown',
    'ExtraSunny',
    'Clear',
    'Clouds',
    'Smog',
    'Foggy',
    'Overcast',
    'Raining',
    'ThunderStorm',
    'Clearing',
    'Neutral',
    'Snowing',
    'Blizzard',
    'Snowlight',
    'Christmas',
    'Halloween'
);


ALTER TYPE weather OWNER TO postgres;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: datasets; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE datasets (
    dataset_id integer NOT NULL,
    dataset_name text,
    view_name text,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE datasets OWNER TO postgres;

--
-- Name: datasets_dataset_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE datasets_dataset_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE datasets_dataset_id_seq OWNER TO postgres;

--
-- Name: datasets_dataset_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE datasets_dataset_id_seq OWNED BY datasets.dataset_id;


--
-- Name: detections; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE detections (
    detection_id integer NOT NULL,
    snapshot_id integer,
    type detection_type,
    pos geometry(PointZ),
    bbox box,
    class detection_class DEFAULT 'Unknown'::detection_class,
    handle integer DEFAULT '-1'::integer,
    best_bbox box,
    best_bbox_old box,
    bbox3d box3d,
    rot geometry,
    coverage real DEFAULT 0.0,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE detections OWNER TO postgres;

--
-- Name: detections_detection_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE detections_detection_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE detections_detection_id_seq OWNER TO postgres;

--
-- Name: detections_detection_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE detections_detection_id_seq OWNED BY detections.detection_id;


--
-- Name: instances; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE instances (
    instance_id integer NOT NULL,
    hostname text,
    instanceid text,
    instancetype text,
    publichostname text,
    amiid text,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE instances OWNER TO postgres;

--
-- Name: instances_instance_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE instances_instance_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE instances_instance_id_seq OWNER TO postgres;

--
-- Name: instances_instance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE instances_instance_id_seq OWNED BY instances.instance_id;


--
-- Name: runs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE runs (
    run_id integer NOT NULL,
    runguid uuid,
    archivepath text,
    localpath text,
    session_id integer DEFAULT 1,
    instance_id integer DEFAULT 0,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE runs OWNER TO postgres;

--
-- Name: runs_run_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE runs_run_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE runs_run_id_seq OWNER TO postgres;

--
-- Name: runs_run_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE runs_run_id_seq OWNED BY runs.run_id;


--
-- Name: sessions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE sessions (
    session_id integer NOT NULL,
    name text,
    start timestamp with time zone,
    "end" timestamp with time zone,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE sessions OWNER TO postgres;

--
-- Name: sessions_session_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE sessions_session_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE sessions_session_id_seq OWNER TO postgres;

--
-- Name: sessions_session_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE sessions_session_id_seq OWNED BY sessions.session_id;


--
-- Name: snapshot_weathers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE snapshot_weathers (
    weather_id integer NOT NULL,
    snapshot_id integer,
    weather_type weather,
    snapshot_page integer,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE snapshot_weathers OWNER TO postgres;

--
-- Name: snapshot_weathers_weather_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE snapshot_weathers_weather_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE snapshot_weathers_weather_id_seq OWNER TO postgres;

--
-- Name: snapshot_weathers_weather_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE snapshot_weathers_weather_id_seq OWNED BY snapshot_weathers.weather_id;


--
-- Name: snapshots; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE snapshots (
    snapshot_id integer NOT NULL,
    run_id integer,
    version integer,
    imagepath text,
    "timestamp" timestamp with time zone,
    timeofday time without time zone,
    currentweather weather,
    camera_pos geometry(PointZ),
    camera_direction geometry,
    camera_fov real,
    view_matrix double precision[],
    proj_matrix double precision[],
    processed boolean DEFAULT false NOT NULL,
    width integer,
    height integer
);


ALTER TABLE snapshots OWNER TO postgres;

--
-- Name: snapshots_snapshot_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE snapshots_snapshot_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE snapshots_snapshot_id_seq OWNER TO postgres;

--
-- Name: snapshots_snapshot_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE snapshots_snapshot_id_seq OWNED BY snapshots.snapshot_id;


--
-- Name: system_graphics; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE system_graphics (
    system_graphic_id integer NOT NULL,
    deviceid text,
    adaptercompatibility text,
    adapterdactype text,
    adapterram integer,
    availability integer,
    caption text,
    description text,
    driverdate timestamp with time zone,
    driverversion text,
    pnpdeviceid text,
    name text,
    videoarch integer,
    memtype integer,
    videoprocessor text,
    bpp integer,
    hrez integer,
    vrez integer,
    num_colors integer,
    cols integer,
    rows integer,
    refresh integer,
    scanmode integer,
    videomodedesc text,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE system_graphics OWNER TO postgres;

--
-- Name: system_graphics_system_graphic_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE system_graphics_system_graphic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE system_graphics_system_graphic_id_seq OWNER TO postgres;

--
-- Name: system_graphics_system_graphic_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE system_graphics_system_graphic_id_seq OWNED BY system_graphics.system_graphic_id;


--
-- Name: systems; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE systems (
    system_uuid uuid NOT NULL,
    vendor text,
    dnshostname text,
    username text,
    systemtype text,
    totalmem bigint,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE systems OWNER TO postgres;

--
-- Name: uploads; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE uploads (
    id integer NOT NULL,
    bucket text,
    key text,
    uploadid text,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE uploads OWNER TO postgres;

--
-- Name: uploads_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE uploads_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE uploads_id_seq OWNER TO postgres;

--
-- Name: uploads_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE uploads_id_seq OWNED BY uploads.id;


--
-- Name: datasets dataset_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY datasets ALTER COLUMN dataset_id SET DEFAULT nextval('datasets_dataset_id_seq'::regclass);


--
-- Name: detections detection_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY detections ALTER COLUMN detection_id SET DEFAULT nextval('detections_detection_id_seq'::regclass);


--
-- Name: instances instance_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY instances ALTER COLUMN instance_id SET DEFAULT nextval('instances_instance_id_seq'::regclass);


--
-- Name: runs run_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY runs ALTER COLUMN run_id SET DEFAULT nextval('runs_run_id_seq'::regclass);


--
-- Name: sessions session_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY sessions ALTER COLUMN session_id SET DEFAULT nextval('sessions_session_id_seq'::regclass);


--
-- Name: snapshot_weathers weather_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY snapshot_weathers ALTER COLUMN weather_id SET DEFAULT nextval('snapshot_weathers_weather_id_seq'::regclass);


--
-- Name: snapshots snapshot_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY snapshots ALTER COLUMN snapshot_id SET DEFAULT nextval('snapshots_snapshot_id_seq'::regclass);


--
-- Name: system_graphics system_graphic_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY system_graphics ALTER COLUMN system_graphic_id SET DEFAULT nextval('system_graphics_system_graphic_id_seq'::regclass);


--
-- Name: uploads id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY uploads ALTER COLUMN id SET DEFAULT nextval('uploads_id_seq'::regclass);

--
-- Name: datasets datasets_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY datasets
    ADD CONSTRAINT datasets_pkey PRIMARY KEY (dataset_id);


--
-- Name: detections detections_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY detections
    ADD CONSTRAINT detections_pkey PRIMARY KEY (detection_id);


--
-- Name: instances instance_info_uniq; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY instances
    ADD CONSTRAINT instance_info_uniq UNIQUE (hostname, instanceid, instancetype, publichostname, amiid);


--
-- Name: instances instanceid_uniq; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY instances
    ADD CONSTRAINT instanceid_uniq UNIQUE (instanceid);


--
-- Name: instances isntances_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY instances
    ADD CONSTRAINT isntances_pkey PRIMARY KEY (instance_id);


--
-- Name: runs runs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY runs
    ADD CONSTRAINT runs_pkey PRIMARY KEY (run_id);


--
-- Name: sessions sessions_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY sessions
    ADD CONSTRAINT sessions_name_key UNIQUE (name);


--
-- Name: sessions sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY sessions
    ADD CONSTRAINT sessions_pkey PRIMARY KEY (session_id);


--
-- Name: snapshot_weathers snapshot_weathers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY snapshot_weathers
    ADD CONSTRAINT snapshot_weathers_pkey PRIMARY KEY (weather_id);


--
-- Name: snapshots snapshots_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY snapshots
    ADD CONSTRAINT snapshots_pkey PRIMARY KEY (snapshot_id);


--
-- Name: system_graphics system_graphics_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY system_graphics
    ADD CONSTRAINT system_graphics_pkey PRIMARY KEY (system_graphic_id);


--
-- Name: systems systems_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY systems
    ADD CONSTRAINT systems_pkey PRIMARY KEY (system_uuid);


--
-- Name: uploads uploads_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY uploads
    ADD CONSTRAINT uploads_pkey PRIMARY KEY (id);


--
-- Name: detections detections_snapshot_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY detections
    ADD CONSTRAINT detections_snapshot_fkey FOREIGN KEY (snapshot_id) REFERENCES snapshots(snapshot_id) ON DELETE CASCADE;


--
-- Name: runs runs_instance_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY runs
    ADD CONSTRAINT runs_instance_fkey FOREIGN KEY (instance_id) REFERENCES instances(instance_id);


--
-- Name: runs runs_session_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY runs
    ADD CONSTRAINT runs_session_fkey FOREIGN KEY (session_id) REFERENCES sessions(session_id) ON DELETE CASCADE;


--
-- Name: snapshot_weathers snapshot_weathers_snapshot_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY snapshot_weathers
    ADD CONSTRAINT snapshot_weathers_snapshot_id_fkey FOREIGN KEY (snapshot_id) REFERENCES snapshots(snapshot_id) ON DELETE CASCADE;


--
-- Name: snapshots snapshots_run_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY snapshots
    ADD CONSTRAINT snapshots_run_fkey FOREIGN KEY (run_id) REFERENCES runs(run_id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

