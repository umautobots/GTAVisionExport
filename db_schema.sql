--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.8
-- Dumped by pg_dump version 9.6.8

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: DATABASE postgres; Type: COMMENT; Schema: -; Owner: postgres
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
-- Name: SCHEMA topology; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON SCHEMA topology IS 'PostGIS Topology schema';


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


--
-- Name: detection_class; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.detection_class AS ENUM (
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


ALTER TYPE public.detection_class OWNER TO postgres;

--
-- Name: detection_type; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.detection_type AS ENUM (
    'background',
    'person',
    'car',
    'bicycle'
);


ALTER TYPE public.detection_type OWNER TO postgres;

--
-- Name: weather; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.weather AS ENUM (
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


ALTER TYPE public.weather OWNER TO postgres;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: datasets; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.datasets (
    dataset_id integer NOT NULL,
    dataset_name text,
    view_name text,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE public.datasets OWNER TO postgres;

--
-- Name: datasets_dataset_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.datasets_dataset_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.datasets_dataset_id_seq OWNER TO postgres;

--
-- Name: datasets_dataset_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.datasets_dataset_id_seq OWNED BY public.datasets.dataset_id;


--
-- Name: detections; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.detections (
    detection_id integer NOT NULL,
    snapshot_id integer,
    type public.detection_type,
    pos public.geometry(PointZ),
    bbox box,
    class public.detection_class DEFAULT 'Unknown'::public.detection_class,
    handle integer DEFAULT '-1'::integer,
    best_bbox box,
    best_bbox_old box,
    bbox3d public.box3d,
    rot public.geometry,
    coverage real DEFAULT 0.0,
    created timestamp without time zone DEFAULT timezone('utc'::text, now()),
    velocity public.geometry(PointZ)
);


ALTER TABLE public.detections OWNER TO postgres;

--
-- Name: detections_detection_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.detections_detection_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.detections_detection_id_seq OWNER TO postgres;

--
-- Name: detections_detection_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.detections_detection_id_seq OWNED BY public.detections.detection_id;


--
-- Name: instances; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.instances (
    instance_id integer NOT NULL,
    hostname text,
    instanceid text,
    instancetype text,
    publichostname text,
    amiid text,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE public.instances OWNER TO postgres;

--
-- Name: instances_instance_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.instances_instance_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.instances_instance_id_seq OWNER TO postgres;

--
-- Name: instances_instance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.instances_instance_id_seq OWNED BY public.instances.instance_id;


--
-- Name: runs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.runs (
    run_id integer NOT NULL,
    runguid uuid,
    archivepath text,
    localpath text,
    session_id integer DEFAULT 1,
    instance_id integer DEFAULT 0,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE public.runs OWNER TO postgres;

--
-- Name: runs_run_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.runs_run_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.runs_run_id_seq OWNER TO postgres;

--
-- Name: runs_run_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.runs_run_id_seq OWNED BY public.runs.run_id;


--
-- Name: sessions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sessions (
    session_id integer NOT NULL,
    name text,
    start timestamp with time zone,
    "end" timestamp with time zone,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE public.sessions OWNER TO postgres;

--
-- Name: sessions_session_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sessions_session_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.sessions_session_id_seq OWNER TO postgres;

--
-- Name: sessions_session_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sessions_session_id_seq OWNED BY public.sessions.session_id;


--
-- Name: snapshot_weathers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.snapshot_weathers (
    weather_id integer NOT NULL,
    snapshot_id integer,
    weather_type public.weather,
    snapshot_page integer,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE public.snapshot_weathers OWNER TO postgres;

--
-- Name: snapshot_weathers_weather_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.snapshot_weathers_weather_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.snapshot_weathers_weather_id_seq OWNER TO postgres;

--
-- Name: snapshot_weathers_weather_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.snapshot_weathers_weather_id_seq OWNED BY public.snapshot_weathers.weather_id;


--
-- Name: snapshots; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.snapshots (
    snapshot_id integer NOT NULL,
    run_id integer,
    version integer,
    scene_id uuid,
    imagepath text,
    "timestamp" timestamp with time zone,
    timeofday time without time zone,
    currentweather public.weather,
    camera_pos public.geometry(PointZ),
    camera_rot public.geometry(PointZ),
    camera_relative_rotation public.geometry(PointZ),
    camera_direction public.geometry,
    camera_fov real,
    world_matrix double precision[],
    view_matrix double precision[],
    proj_matrix double precision[],
    processed boolean DEFAULT false NOT NULL,
    width integer,
    height integer,
    ui_width integer,
    ui_height integer,
    cam_near_clip real,
    cam_far_clip real,
    player_pos public.geometry(PointZ),
    velocity public.geometry(PointZ),
    camera_relative_position public.geometry(PointZ)
);


ALTER TABLE public.snapshots OWNER TO postgres;

--
-- Name: snapshots_snapshot_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.snapshots_snapshot_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.snapshots_snapshot_id_seq OWNER TO postgres;

--
-- Name: snapshots_snapshot_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.snapshots_snapshot_id_seq OWNED BY public.snapshots.snapshot_id;


--
-- Name: snapshots_snapshot_id_seq1; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.snapshots_snapshot_id_seq1
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.snapshots_snapshot_id_seq1 OWNER TO postgres;

--
-- Name: snapshots_snapshot_id_seq1; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.snapshots_snapshot_id_seq1 OWNED BY public.snapshots.snapshot_id;


--
-- Name: snapshots_view; Type: VIEW; Schema: public; Owner: gta_v
--

CREATE VIEW public.snapshots_view AS
 SELECT snapshots.snapshot_id,
    snapshots.run_id,
    snapshots.version,
    snapshots.scene_id,
    snapshots.imagepath,
    snapshots."timestamp",
    snapshots.timeofday,
    snapshots.currentweather,
    ARRAY[public.st_x(snapshots.camera_pos), public.st_y(snapshots.camera_pos), public.st_z(snapshots.camera_pos)] AS camera_pos,
    ARRAY[public.st_x(snapshots.camera_rot), public.st_y(snapshots.camera_rot), public.st_z(snapshots.camera_rot)] AS camera_rot,
    ARRAY[public.st_x(snapshots.camera_relative_rotation), public.st_y(snapshots.camera_relative_rotation), public.st_z(snapshots.camera_relative_rotation)] AS camera_relative_rotation,
    ARRAY[public.st_x(snapshots.camera_relative_position), public.st_y(snapshots.camera_relative_position), public.st_z(snapshots.camera_relative_position)] AS camera_relative_position,
    ARRAY[public.st_x(snapshots.camera_direction), public.st_y(snapshots.camera_direction), public.st_z(snapshots.camera_direction)] AS camera_direction,
    snapshots.camera_fov,
    snapshots.world_matrix,
    snapshots.view_matrix,
    snapshots.proj_matrix,
    snapshots.processed,
    snapshots.width,
    snapshots.height,
    snapshots.ui_width,
    snapshots.ui_height,
    snapshots.cam_near_clip,
    snapshots.cam_far_clip,
    ARRAY[public.st_x(snapshots.player_pos), public.st_y(snapshots.player_pos), public.st_z(snapshots.player_pos)] AS player_pos,
    ARRAY[public.st_x(snapshots.velocity), public.st_y(snapshots.velocity), public.st_z(snapshots.velocity)] AS velocity
   FROM public.snapshots;


ALTER TABLE public.snapshots_view OWNER TO gta_v;

--
-- Name: system_graphics; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.system_graphics (
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


ALTER TABLE public.system_graphics OWNER TO postgres;

--
-- Name: system_graphics_system_graphic_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.system_graphics_system_graphic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.system_graphics_system_graphic_id_seq OWNER TO postgres;

--
-- Name: system_graphics_system_graphic_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.system_graphics_system_graphic_id_seq OWNED BY public.system_graphics.system_graphic_id;


--
-- Name: systems; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.systems (
    system_uuid uuid NOT NULL,
    vendor text,
    dnshostname text,
    username text,
    systemtype text,
    totalmem bigint,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE public.systems OWNER TO postgres;

--
-- Name: uploads; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.uploads (
    id integer NOT NULL,
    bucket text,
    key text,
    uploadid text,
    created timestamp without time zone DEFAULT timezone('utc'::text, now())
);


ALTER TABLE public.uploads OWNER TO postgres;

--
-- Name: uploads_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.uploads_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.uploads_id_seq OWNER TO postgres;

--
-- Name: uploads_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.uploads_id_seq OWNED BY public.uploads.id;


--
-- Name: datasets dataset_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.datasets ALTER COLUMN dataset_id SET DEFAULT nextval('public.datasets_dataset_id_seq'::regclass);


--
-- Name: detections detection_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detections ALTER COLUMN detection_id SET DEFAULT nextval('public.detections_detection_id_seq'::regclass);


--
-- Name: instances instance_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instances ALTER COLUMN instance_id SET DEFAULT nextval('public.instances_instance_id_seq'::regclass);


--
-- Name: runs run_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.runs ALTER COLUMN run_id SET DEFAULT nextval('public.runs_run_id_seq'::regclass);


--
-- Name: sessions session_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sessions ALTER COLUMN session_id SET DEFAULT nextval('public.sessions_session_id_seq'::regclass);


--
-- Name: snapshot_weathers weather_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.snapshot_weathers ALTER COLUMN weather_id SET DEFAULT nextval('public.snapshot_weathers_weather_id_seq'::regclass);


--
-- Name: snapshots snapshot_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.snapshots ALTER COLUMN snapshot_id SET DEFAULT nextval('public.snapshots_snapshot_id_seq'::regclass);


--
-- Name: system_graphics system_graphic_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_graphics ALTER COLUMN system_graphic_id SET DEFAULT nextval('public.system_graphics_system_graphic_id_seq'::regclass);


--
-- Name: uploads id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.uploads ALTER COLUMN id SET DEFAULT nextval('public.uploads_id_seq'::regclass);


--
-- Name: datasets datasets_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.datasets
    ADD CONSTRAINT datasets_pkey PRIMARY KEY (dataset_id);


--
-- Name: detections detections_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detections
    ADD CONSTRAINT detections_pkey PRIMARY KEY (detection_id);


--
-- Name: instances instance_info_uniq; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instances
    ADD CONSTRAINT instance_info_uniq UNIQUE (hostname, instanceid, instancetype, publichostname, amiid);


--
-- Name: instances instanceid_uniq; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instances
    ADD CONSTRAINT instanceid_uniq UNIQUE (instanceid);


--
-- Name: instances isntances_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instances
    ADD CONSTRAINT isntances_pkey PRIMARY KEY (instance_id);


--
-- Name: runs runs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.runs
    ADD CONSTRAINT runs_pkey PRIMARY KEY (run_id);


--
-- Name: sessions sessions_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_name_key UNIQUE (name);


--
-- Name: sessions sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_pkey PRIMARY KEY (session_id);


--
-- Name: snapshot_weathers snapshot_weathers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.snapshot_weathers
    ADD CONSTRAINT snapshot_weathers_pkey PRIMARY KEY (weather_id);


--
-- Name: snapshots snapshots_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.snapshots
    ADD CONSTRAINT snapshots_pkey PRIMARY KEY (snapshot_id);


--
-- Name: system_graphics system_graphics_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_graphics
    ADD CONSTRAINT system_graphics_pkey PRIMARY KEY (system_graphic_id);


--
-- Name: systems systems_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.systems
    ADD CONSTRAINT systems_pkey PRIMARY KEY (system_uuid);


--
-- Name: uploads uploads_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.uploads
    ADD CONSTRAINT uploads_pkey PRIMARY KEY (id);


--
-- Name: handle_index; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX handle_index ON public.detections USING btree (handle);


--
-- Name: snapshot_index; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX snapshot_index ON public.detections USING btree (snapshot_id);


--
-- Name: detections detections_snapshot_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detections
    ADD CONSTRAINT detections_snapshot_fkey FOREIGN KEY (snapshot_id) REFERENCES public.snapshots(snapshot_id) ON DELETE CASCADE;


--
-- Name: runs runs_instance_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.runs
    ADD CONSTRAINT runs_instance_fkey FOREIGN KEY (instance_id) REFERENCES public.instances(instance_id);


--
-- Name: runs runs_session_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.runs
    ADD CONSTRAINT runs_session_fkey FOREIGN KEY (session_id) REFERENCES public.sessions(session_id) ON DELETE CASCADE;


--
-- Name: snapshot_weathers snapshot_weathers_snapshot_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.snapshot_weathers
    ADD CONSTRAINT snapshot_weathers_snapshot_id_fkey FOREIGN KEY (snapshot_id) REFERENCES public.snapshots(snapshot_id) ON DELETE CASCADE;


--
-- Name: snapshots snapshots_run_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.snapshots
    ADD CONSTRAINT snapshots_run_fkey FOREIGN KEY (run_id) REFERENCES public.runs(run_id) ON DELETE CASCADE;


--
-- Name: TABLE datasets; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.datasets TO gta_v;


--
-- Name: SEQUENCE datasets_dataset_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.datasets_dataset_id_seq TO gta_v;


--
-- Name: TABLE detections; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.detections TO gta_v;


--
-- Name: SEQUENCE detections_detection_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.detections_detection_id_seq TO gta_v;


--
-- Name: TABLE geography_columns; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.geography_columns TO gta_v;


--
-- Name: TABLE geometry_columns; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.geometry_columns TO gta_v;


--
-- Name: TABLE instances; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.instances TO gta_v;


--
-- Name: SEQUENCE instances_instance_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.instances_instance_id_seq TO gta_v;


--
-- Name: TABLE raster_columns; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.raster_columns TO gta_v;


--
-- Name: TABLE raster_overviews; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.raster_overviews TO gta_v;


--
-- Name: TABLE runs; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.runs TO gta_v;


--
-- Name: SEQUENCE runs_run_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.runs_run_id_seq TO gta_v;


--
-- Name: TABLE sessions; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.sessions TO gta_v;


--
-- Name: SEQUENCE sessions_session_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.sessions_session_id_seq TO gta_v;


--
-- Name: TABLE snapshot_weathers; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.snapshot_weathers TO gta_v;


--
-- Name: SEQUENCE snapshot_weathers_weather_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.snapshot_weathers_weather_id_seq TO gta_v;


--
-- Name: TABLE snapshots; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.snapshots TO gta_v;


--
-- Name: SEQUENCE snapshots_snapshot_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.snapshots_snapshot_id_seq TO gta_v;


--
-- Name: SEQUENCE snapshots_snapshot_id_seq1; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.snapshots_snapshot_id_seq1 TO gta_v;


--
-- Name: TABLE spatial_ref_sys; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.spatial_ref_sys TO gta_v;


--
-- Name: TABLE system_graphics; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.system_graphics TO gta_v;


--
-- Name: SEQUENCE system_graphics_system_graphic_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.system_graphics_system_graphic_id_seq TO gta_v;


--
-- Name: TABLE systems; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.systems TO gta_v;


--
-- Name: TABLE uploads; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.uploads TO gta_v;


--
-- Name: SEQUENCE uploads_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON SEQUENCE public.uploads_id_seq TO gta_v;


--
-- PostgreSQL database dump complete
--

