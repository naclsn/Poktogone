<h1>Votre compte (<?=$user['name']?>)</h1>

<p>Elo : <?=$user['elo']?></p>

<form action="user.php" method="post">
    <input type="hidden" name="action" value="edit">

    <input type="text" name="name" id="edit-name" value="<?=$user['name']?>" placeholder="Entrez un nouveau nom">
    <input type="password" name="name" id="edit-pass" value="***" placeholder="Entrez un nouveau mot de passe">

    <button type="submit">Appliquer</button>
</form>
