<?php
    /**
     * actions:
     * - continue (id, ...args) // continue the battle, return battle infos
     * - create (id)            // create a battle, wait for user [id] to answer
     * - answer (id)            // answer to a created battle, create the process
     * - [default] show (id)    // show spectators details of the battle [id]
     */
    require_once "./includes/sess.php";

    $id = @$_REQUEST['id'];
    $args = @$_REQUEST['args'];

    switch (@$_REQUEST['action']) {

        case "continue":
            break;

        case "create":
                if (empty($_SESSION['id']) || $_SESSION['id'] == $id) {
                    $error = "Invalid action";
                    break;
                }

                $mate = $db_user->select(
                        'users',
                        array('id', 'name'),
                        array('id' => $id)
                    )->fetch();
                if (empty($mate)) {
                    $error = "User not found";
                    break;
                }

                /*if ($db_user->select('battles', "*", array(
                        'P1' => array($_SESSION['id'], $mate['id']),
                        'P2' => array($_SESSION['id'], $mate['id']),
                        'status !' => null
                    ))) {
                    $error = "Already in battle";
                    break;
                }*/

                $id = $db_user->insert(
                        'battles',
                        array(
                                'id' => null,
                                'date' => "NOW()",
                                'winner' => null,
                                'loser' => null,
                                'P1' => $_SESSION['id'],
                                'P2' => $mate['id'],
                                'status' => "..."
                            ),
                        null,
                        'id'
                    );
            break;

        case "answer":
                if (@$_SESSION['id'] == ($battle = @$db_user->select('battles', "*", array('id' => $id))->fetch())['P2']) {
                    if ($battle['status'] == "...") {
                        $p1_name = $db_user->select('users', 'name', array('id' => $battle['P1']))->fetch()['name'];
                        $p2_name = $db_user->select('users', 'name', array('id' => $battle['P2']))->fetch()['name'];
                        $p1_team = "2;3;4"; //$battle['P1_team'];
                        $p2_team = "21;22;23"; //$battle['P2_team'];

                        echo "<br>(<code>cmd /c .\\prog\\Poktogone.exe \"$p1_name\" \"$p1_team\" \"$p2_name\" \"$p2_team\" < .\\prog\\in\\$id.txt > .\\prog\\out\\$id.txt 2> .\\prog\\err\\$id.txt</code>)<br>";
                        echo `cmd /c .\\prog\\Poktogone.exe "$p1_name" "$p1_team" "$p2_name" "$p2_team" < .\\prog\\in\\$id.txt > .\\prog\\out\\$id.txt 2> .\\prog\\err\\$id.txt`;

                        //$o = proc_open();

                        $db_user->update('battles', array('status' => null), array('id' => $battle['id']));
                    } else $error = "Battle not in starting stat";
                } else $error = "Invalid action";
            break;

        default: //case "show":
                if (empty($id)) {
                    if (empty($_SESSION['id'])) { // TODO: show every on-going battles ?
                        $error = "Invalid action";
                        break;
                    }

                    $r = $db_user->select(
                            array('battles', 'users'),
                            array('users.name', 'users.id', 'battles.*'),
                            array(
                                    'users::id !' => $_SESSION['id'],
                                    'battles::P1' => array('users::id', $_SESSION['id']),
                                    'battles::P2' => array('users::id', $_SESSION['id']),
                                    'battles::status !' => null
                                )
                        ); ?>

                    <ul class="on-going">
                        <?php
                            while ($battle = $r->fetch()) { ?>
                                <li>
                                    <a href="./battle.php?id=<?=$battle['id']?>">Combat en cours contre <?=$battle['name']?></a>
                                    <p><?php var_dump($battle) ?></p>
                                </li><?php
                            } ?>
                    </ul><?php

                    break;
                }

                $battle = $db_user->select('battles', "*", array('id' => @$id));

                if (empty($battle)) {
                    $error = "Battle not found";
                    break;
                }

                $status = $battle['status'];
                include "./includes/battle/status.php";
    }

    if (@$error) {
        $_SESSION['error'] = $error;
        header("Location: ./#");
        exit;
    }
?>
