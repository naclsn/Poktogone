<h1>Compte de <?=$user['name']?></h1>

<p>Elo : <?=$user['elo']?></p>

<div class="container-fluid">
    <?php
        $r = $db_user->select(
                    array('users', 'battles'),
                    array('users.name', 'users.id', 'battles.date', 'battles.winner', 'battles.loser'),
                    array(
                            'users::id !' => $user['id'],
                            'battles::loser' => array('users::id', $user['id']),
                            'battles::winner' => array('users::id', $user['id'])
                        ),
                    array('users::name' => 'ASC', 'battles::date' => 'DESC')
            );

        if (0 < $r->rowCount()) { ?>
            <div id="accordion">
                <?php
                    $mate = $r->fetch();
                    while ($mate) { ?>
                        <div class="card">
                            <div class="card-header" id="headingOne">
                                <h5 class="mb-0">
                                    <button class="btn btn-link" data-toggle="collapse" data-target="#collapse<?=$mate['id']?>" aria-expanded="true" aria-controls="collapse<?=$mate['id']?>">
                                        Afficher les combats contre <?=$mate['name']?>
                                    </button>
                                </h5>
                            </div>

                            <div id="collapse<?=$mate['id']?>" class="collapse" aria-labelledby="headingOne" data-parent="#accordion">
                                <?php
                                    $battle = $mate;
                                    do {
                                        $won = $battle['winner'] == $user['id']; ?>
                                        <div class="card-body <?=$won ? "bg-success" : "bg-danger"?>">
                                            <?=$won ? "Victoire" : "Defaite"?> le <?=$battle['date']?>
                                        </div><?php
                                    } while (($battle = $r->fetch()) && $battle['name'] == $mate['name']);
                                    $mate = $battle;
                                ?>
                            </div>
                        </div><?php
                    }
                ?>
            </div><?php
        } else { ?>
            <p>Ce joueur n'a pas fait de parties...</p><?php
        }
    ?>
</div>
