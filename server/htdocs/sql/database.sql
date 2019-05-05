CREATE DATABASE IF NOT EXISTS poktogone_user CHARACTER SET utf8 collate utf8_unicode_ci;
USE poktogone_user;

GRANT all privileges ON poktogone_user.* TO 'poktogone_user_as'@'localhost' identified BY 'admin';
