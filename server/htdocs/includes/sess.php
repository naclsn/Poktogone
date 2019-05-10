<?php
    session_start();

    require_once "./includes/conx/conx.php";
    require_once "./includes/conx/conx_da.php";

    $db_user = new ConX("poktogone_user");

    //$db_poke = new ConX("Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=./prog/Database.mdf;Integrated Security=True", false);
    //$db_poke = new ConX("sqlsrv:server=(LocalDB)\\v17.0;AttachDBFileName=.\\prog\\Database.mdf", false);//"sqlsrv:server=(localdb)\\v11.0;AttachDBFileName=c:\\myData.MDF"
    $db_poke = new ConX("sqlsrv:server=(LocalDB)\\MSSQLLocalDB;AttachDBFileName=C:\\xampp\\htdocs\\Poktogone\\prog\\Database.mdf", false);
    $db_poke->set_escape('[', ']');
?>
