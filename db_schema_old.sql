SET search_path = public, pg_catalog;


create type detection_class as enum ('Unknown', 'Compacts', 'Sedans', 'SUVs', 'Coupes', 'Muscle', 'SportsClassics', 'Sports', 'Super', 'Motorcycles', 'OffRoad', 'Industrial', 'Utility', 'Vans', 'Cycles', 'Boats', 'Helicopters', 'Planes', 'Service', 'Emergency', 'Military', 'Commercial', 'Trains')
;

create type detection_type as enum ('background', 'person', 'car', 'bicycle')
;

create type weather as enum ('Unknown', 'ExtraSunny', 'Clear', 'Clouds', 'Smog', 'Foggy', 'Overcast', 'Raining', 'ThunderStorm', 'Clearing', 'Neutral', 'Snowing', 'Blizzard', 'Snowlight', 'Christmas', 'Halloween')
;


create table datasets
(
	dataset_id serial not null
		constraint datasets_pkey
			primary key,
	dataset_name text,
	view_name text,
	created timestamp default timezone('utc'::text, now())
)
;

create table instances
(
	instance_id serial not null
		constraint isntances_pkey
			primary key,
	hostname text,
	instanceid text
		constraint instanceid_uniq
			unique,
	instancetype text,
	publichostname text,
	amiid text,
	created timestamp default timezone('utc'::text, now()),
	constraint instance_info_uniq
		unique (hostname, instanceid, instancetype, publichostname, amiid)
)
;

create table sessions
(
	session_id serial not null
		constraint sessions_pkey
			primary key,
	name text
		constraint sessions_name_key
			unique,
	start timestamp with time zone,
	"end" timestamp with time zone,
	created timestamp default timezone('utc'::text, now())
)
;

create table runs
(
	run_id serial not null
		constraint runs_pkey
			primary key,
	runguid uuid,
	archivepath text,
	localpath text,
	session_id integer default 1
		constraint runs_session_fkey
			references sessions
				on delete cascade,
	instance_id integer default 0
		constraint runs_instance_fkey
			references instances,
	created timestamp default timezone('utc'::text, now())
)
;

create table snapshots
(
	snapshot_id serial not null
		constraint snapshots_pkey
			primary key,
	run_id integer
		constraint snapshots_run_fkey
			references runs
				on delete cascade,
	version integer,
	scene_id uuid,
	imagepath text,
	timestamp timestamp with time zone,
	timeofday time,
	currentweather weather,
	camera_pos geometry(PointZ),
	camera_rot geometry(PointZ),
	camera_relative_rotation geometry(PointZ),
	camera_direction geometry,
	camera_fov real,
	world_matrix double precision[],
	view_matrix double precision[],
	proj_matrix double precision[],
	processed boolean default false not null,
	width integer,
	height integer,
	ui_width integer,
	ui_height integer,
	cam_near_clip real,
	cam_far_clip real,
	player_pos geometry(PointZ),
	velocity geometry(PointZ),
	camera_relative_position geometry(PointZ)
)
;

create table detections
(
	detection_id serial not null
		constraint detections_pkey
			primary key,
	snapshot_id integer
		constraint detections_snapshot_fkey
			references snapshots
				on delete cascade,
	type detection_type,
	pos geometry(PointZ),
	bbox box,
	class detection_class default 'Unknown'::detection_class,
	handle integer default '-1'::integer,
	best_bbox box,
	best_bbox_old box,
	bbox3d box3d,
	rot geometry,
	coverage real default 0.0,
	created timestamp default timezone('utc'::text, now()),
	velocity geometry(PointZ)
)
;

create index snapshot_index
	on detections (snapshot_id)
;

create index handle_index
	on detections (handle)
;

create table snapshot_weathers
(
	weather_id serial not null
		constraint snapshot_weathers_pkey
			primary key,
	snapshot_id integer
		constraint snapshot_weathers_snapshot_id_fkey
			references snapshots
				on delete cascade,
	weather_type weather,
	snapshot_page integer,
	created timestamp default timezone('utc'::text, now())
)
;

create table system_graphics
(
	system_graphic_id serial not null
		constraint system_graphics_pkey
			primary key,
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
	created timestamp default timezone('utc'::text, now())
)
;

create table systems
(
	system_uuid uuid not null
		constraint systems_pkey
			primary key,
	vendor text,
	dnshostname text,
	username text,
	systemtype text,
	totalmem bigint,
	created timestamp default timezone('utc'::text, now())
)
;

create table uploads
(
	id serial not null
		constraint uploads_pkey
			primary key,
	bucket text,
	key text,
	uploadid text,
	created timestamp default timezone('utc'::text, now())
)
;

