<?php
    session_start();

    require_once "./includes/conx/conx.php";
    require_once "./includes/conx/conx_da.php";

    $db_user = new ConX("poktogone_user");
    //$db_user->debug = true;
    //$db_poke = new ConX("poktogone_poke");
?>
