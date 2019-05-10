<?php
    require_once "./includes/sess.php";
?>

<?php include "./includes/head.php" ?>

<h1>Hay!</h1>

<ul>
    <?php
        $r = $db_poke->select('pokemons');
        while ($p = $r->fetch()) {
            $type1 = $db_poke->select('types', 'name', array('id' => $p['type1']))->fetch()['name'];
            $type2 = $db_poke->select('types', 'name', array('id' => $p['type2']))->fetch()['name'];

            $type1 = "Type 1 : $type1";
            if ($type2)
                $type2 = "Type 2 : $type2"?>
            <li>
                <h4><?=$p['name']?></h4>
                <p><?=$type1.($type2 ? "<br>$type2" : "")?></p>
            </li><?php
        }
    ?>
</ul>


<?php include "./includes/foot.php" ?>
