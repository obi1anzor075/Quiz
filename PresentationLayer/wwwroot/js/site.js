<script>
    document.addEventListener('DOMContentLoaded', () => {
        const backButton = document.getElementById('backButton');

    if (backButton) {
        backButton.addEventListener('click', () => {
            // Отправляем запрос на сброс счетчика верных ответов и CurrentQuestionId
            fetch('/Game/ResetCounters')

                .catch(error => {
                    console.error('Error:', error);
                });
        });
        }
    });
</script>
