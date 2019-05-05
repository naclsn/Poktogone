<!DOCTYPE html>
<html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">

        <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" integrity="sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T" crossorigin="anonymous">
        <link rel="stylesheet" href="./css/style.css">

        <title>POKTOGONE<?=@$__sub_title ? " &mdash; $__sub_title" : ""?></title>

        <script src="http://code.jquery.com/jquery-latest.min.js"></script>
    </head>

    <body>
        <nav class="navbar navbar-expand-lg navbar-light bg-light sticky-top">
            <div class="container">
                <a class="navbar-brand" href="./#"><u>&nbsp;Poktogone&nbsp;</u></a>
                
                <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                    <span class="navbar-toggler-icon"></span>
                </button>

                <div class="collapse navbar-collapse" id="navbarSupportedContent">
                    <div class="container float-right">
                        <?php
                            if (empty($_SESSION['id'])) { ?>
                                <form class="form-inline float-right" action="user.php" method="post">
                                    <label class="sr-only" for="loginName">Nom</label>
                                    <input type="text" class="form-control mr-sm-2" name="name" id="loginName" placeholder="Nom">

                                    <label class="sr-only" for="loginPass">Mot de passe</label>
                                    <div class="input-group mr-sm-2">
                                        <div class="input-group-prepend"><div class="input-group-text">#</div></div>
                                        <input type="password" class="form-control" name="pass" id="loginPass" placeholder="Mot de passe">
                                    </div>

                                    <!--div class="form-check mr-sm-2">
                                        <input class="form-check-input" type="checkbox" id="loginRemember">
                                        <label class="form-check-label" name="remember" for="loginRemember" title="&Ccedil;a sert &agrave; rien, c'est pas impl&eacute;ment&eacute;">Rester connecter</label>
                                    </div-->

                                    <div class="input-group mr-sm-2">
                                        <button class="btn btn-primary" type="submit" name="action" value="login">Se Connecter</button>
                                    </div>
                                    <div class="input-group mr-sm-2">
                                        <button class="btn btn-secondary" type="submit" name="action" value="signin">Cr&eacute;er le Compte</button>
                                    </div>
                                </form><?php
                            } else { ?>
                                <div class="float-right">
                                    <div class="text-success">
                                        Hey, <a href="./user.php?id=<?=$_SESSION['id']?>" title="Détails du coupte"><?=$_SESSION['name']?></a> !&nbsp;
                                        <a href="./user.php?action=logout" class="btn btn-danger">Se Déonnecter</a>
                                    </div>
                                </div><?php
                            }
                        ?>
                    </div>
                </div> <!-- /.collapse -->
            </div>
        </nav>

        <div class="container-fluid page">
            <?php
                if (@$_SESSION['error']) {?>
                    <div class="alert alert-danger" role="alert">
                        Erreur : &apos;<code><?=$_SESSION['error']?></code>&apos;
                        <button type="button" class="close" data-dismiss="alert" aria-label="Fermer">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div><?php
                    unset($_SESSION['error']);
                }

                if (@$_SESSION['info']) {?>
                    <div class="alert alert-success" role="alert">
                        Information : &apos;<code><?=$_SESSION['info']?></code>&apos;
                        <button type="button" class="close" data-dismiss="alert" aria-label="Fermer">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div><?php
                    unset($_SESSION['info']);
                }
            ?>
