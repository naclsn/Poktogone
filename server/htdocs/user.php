<?php
    /**
     * actions:
     * - logout ()           // destroy the current session, if any
     * - signin (name, pass) // sign a new user in, if name available
     * - login (name, pass)  // log the user in, if name and pass agree
     * - edit (id, name)     // edit the user [id]'s name and pass
     * - [default] show (id) // show details of the user [id]
     */
    require_once "./includes/sess.php";

    $name = htmlentities(@$_REQUEST['name']);
    $pass = @$_REQUEST['pass'];
    $id = @$_REQUEST['id'];

    switch (@$_REQUEST['action']) {

        case "logout":
                unset($_SESSION);
                session_destroy();

                $_SESSION['info'] = "Session destroyed";
                header("Location: ./#");
            break;

        case "edit":
                if (empty($_SESSION['id'])) {
                    $error = "Invalid action";
                    break;
                }

                if (empty($pass))
                    $what = array('name' => $name);
                else $what = array(
                            'name' => $name,
                            'pass' => password_hash($pass, PASSWORD_DEFAULT)
                        );

                $db_user->update('users', $what, array('id' => $_SESSION['id']));

                $_SESSION['info'] = "Profil updated";
                header("Location: ./#");
            break;

        case "signin":
                if ($db_user->select('users', "name", array('name' => $name))->fetch()) {
                    $error = "Account exists";
                    break;
                }

                $db_user->insert('users', array(
                        'id' => null,
                        'name' => $name,
                        'pass' => password_hash($pass, PASSWORD_DEFAULT),
                        'elo' => 1000
                    ));
            //break;

        case "login":
                $r = $db_user->select('users', "*", array('name' => $name));
                if ($user = $r->fetch()) {
                    if (password_verify($pass, $user['pass'])) {
                        $_SESSION['id'] = $user['id'];
                        $_SESSION['name'] = $user['name'];
                        $_SESSION['elo'] = $user['elo'];
                        $id = $_SESSION['id'];
                    } else $error = "Wrong password";
                } else $error = "User not found";

                if (@$error) break;
            //break;

        default: //case "show":
                if (empty($id))
                    $user = $name === @$_SESSION['name'] ? $_SESSION : $db_user->select('users', "*", array('name' => $name))->fetch();
                else $user = $id === @$_SESSION['id'] ? $_SESSION : $db_user->select('users', "*", array('id' => $id))->fetch();

                if (empty($user)) {
                    $error = "User not found";
                    break;
                }

                $__sub_title = $user['name'];
                include "./includes/head.php";

                if ($user['id'] == @$_SESSION['id'])
                    include "./includes/account/me.php";
                else include "./includes/account/generic.php";

                include "./includes/foot.php";
    }

    if (@$error) {
        $_SESSION['error'] = $error;
        header("Location: ./#");
        exit;
    }
?>
