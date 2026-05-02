public enum GameState
{
    MainMenu,   // Reservado para el futuro — por ahora el juego empieza directo
    Playing,    // El jugador está en una run activa
    Paused,     // Reservado para el futuro
    Death,      // El jugador acaba de morir — muestra Game Over
    Upgrades    // El jugador está en la pantalla de upgrades entre runs
}