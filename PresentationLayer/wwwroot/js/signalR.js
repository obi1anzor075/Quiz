$(document).ready(function () {
    let chatRoom = '';
    let selectedAnswer = '';
    let userName = '';
    let isJoining = false;
    let timerInterval;
    localStorage.setItem('userName', userName);

    // Установка соединения с хабом SignalR
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub") // Укажите URL вашего хаба SignalR
        .build();

    connection.start().then(function () {
        console.log("SignalR connection established.");

        // Получение имени пользователя из URL
         userName = getParameterByName('userName');
        connection.invoke("SetUserName", getParameterByName('userName'));
        if (userName) {
            userName = userName;
            joinChat(userName);
        }
    }).catch(function (err) {
        console.error(err.toString());
    });

    // Функция для получения параметра из URL
    function getParameterByName(name) {
        name = name.replace(/[\[\]]/g, '\\$&');
        var url = window.location.href;
        var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)');
        var results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, ' '));
    }

    // Функция для присоединения к чату
    function joinChat(userName) {
        var connectionInfo = {
            userName: userName,
            chatRoom: "Global" // Укажите нужную чат-комнату
        };

        connection.invoke("JoinChat", connectionInfo).then(function () {
            $(".chat-box").show('scale');
            $(".chat-log").hide('scale');
            $("#chat-circle").hide(); // Скрыть chat circle при показе chat box

            // Отправка сообщения "добро пожаловать"
        }).catch(function (err) {
            console.error(err.toString());
        });
    }

    // Функция для отправки сообщения
    function sendMessage(userName, message) {
        connection.invoke("SendMessage", userName, message).then(function () {
            console.log("Message sent.");
        }).catch(function (err) {
            console.error(err.toString());
        });
    }

    // Обработка отправки сообщения из chat box
    $("#chat-submit").click(function (e) {
        e.preventDefault();
        var message = $("#chat-input").val();
        var userName = getParameterByName('userName'); // Получаем имя пользователя из URL
        if (message.trim() !== '') {
            sendMessage(userName, message);
            $("#chat-input").val('');

            // Перемещение вниз чата с использованием requestAnimationFrame
            requestAnimationFrame(function () {
                const chat = document.getElementById("chat-logs");
                chat.scrollTop = chat.scrollHeight;
            });
        }
    });

    // Обработка получения сообщения от сервера
    connection.on("ReceiveMessage", function (userName, message) {
        $(".chat-logs").append(generateMessageHTML(userName, message));

        // Перемещение вниз чата при получении нового сообщения
        requestAnimationFrame(function () {
            const chat = document.getElementById("chat-logs");
            chat.scrollTop = chat.scrollHeight;
        });
    });

    // Вспомогательная функция для генерации HTML кода сообщения
    function generateMessageHTML(userName, message) {
        const avatarSrc = getAvatar(userName);

        return '<div class="chat-msg">' +
            '<span class="msg-avatar">' +
            '<img class="avatar-img" src="' + avatarSrc + '" alt="avatar">' +
            '</span>' +
            '<div class="cm-msg-text"><strong>' + userName + ':</strong> ' + message + '</div>' +
            '</div>';
    }

    // Список случайных аватаров
    const randomAvatars = [
        '/img/chat-logo/logo1.svg',
        '/img/chat-logo/logo2.svg',
        '/img/chat-logo/logo3.svg',
        '/img/chat-logo/logo4.svg'
    ];

    // Функция для генерации или получения сохраненного аватара
    function getAvatar(userName) {
        let avatar = localStorage.getItem(`avatar_${userName}`);
        if (!avatar) {
            avatar = generateAvatar(userName);
            localStorage.setItem(`avatar_${userName}`, avatar);
        }
        return avatar;
    }

    // Функция для генерации аватара
    function generateAvatar(userName) {
        if (userName === 'Brand-Battle') {
            return '/img/chat-logo/Admin-logo.jpg'; // Специальный аватар для BrandBattle
        } else {
            const randomIndex = Math.floor(Math.random() * randomAvatars.length);
            return randomAvatars[randomIndex];
        }
    }

    // Обработчики событий для chat circle и chat box toggle
    $("#chat-circle").click(function () {
        chatCircle.hide('scale');
        chatBox.show('scale');
    });

    $(".chat-box-toggle").click(function () {
        chatBox.hide('scale', function () {
            chatCircle.show('scale'); // Показать chat circle после скрытия chat box
        });
    });

    // Остальной код, связанный с игрой
    connection.on("ReceiveQuestion", function (questionId, questionText, imageUrl, answers) {
        console.log("Received question:", questionId, questionText, imageUrl, answers); // Logging received data

        // Установка текста вопроса и атрибута data-question-id
        var questionTextElement = document.getElementById('questionText');
        var questionImageElement = document.getElementById('questionImage');
        var answersListElement = document.getElementById('answersList');

        if (questionTextElement) {
            questionTextElement.innerText = questionText;
            questionTextElement.setAttribute('data-question-id', questionId);
        }

        // Отображение изображения вопроса
        if (questionImageElement) {
            if (imageUrl) {
                questionImageElement.src = "/img/questions/" + imageUrl;
                questionImageElement.style.display = 'block';
            } else {
                questionImageElement.style.display = 'none';
            }
        }

        // Очистка и отображение списка ответов
        if (answersListElement) {
            answersListElement.innerHTML = '';

            // Shuffle answers randomly
            answers = shuffle(answers);

            const column1 = document.createElement('div');
            const column2 = document.createElement('div');
            column1.classList.add('wrapper-answer');
            column2.classList.add('wrapper-answer');

            answers.forEach((answer, index) => {
                const li = document.createElement('li');
                li.textContent = answer;
                li.classList.add('answer');
                li.classList.add('eventListener');
                li.addEventListener('click', async function () {
                    selectedAnswer = answer;
                    document.querySelectorAll('#answersList .answer').forEach(item => {
                        item.classList.remove('selected');
                    });
                    li.classList.add('selected');
                    await submitAnswer(questionId, selectedAnswer);
                });

                // Добавляем ответ в соответствующую колонку
                if (index % 2 === 0) {
                    column1.appendChild(li);
                } else {
                    column2.appendChild(li);
                }
            });

            // Добавляем колонки с ответами в список
            answersListElement.appendChild(column1);
            answersListElement.appendChild(column2);

            // Запуск таймера только при получении первого вопроса
            if (!timerInterval) {
                startTimer(45); // Specify the duration in seconds
            }
        }
    });

    // Function to shuffle array randomly
    function shuffle(array) {
        for (let i = array.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [array[i], array[j]] = [array[j], array[i]];
        }
        return array;
    }

    connection.on("AnswerResult", function (isCorrect) {
        const selectedElement = document.querySelector('.answer.selected');
        if (selectedElement) {
            selectedElement.classList.add(isCorrect ? 'correct' : 'incorrect');
        }
        getNextQuestion(); // Запрашиваем следующий вопрос сразу после получения результата ответа
    });

    connection.on("GameReady", function () {
        console.log("Both players are ready!");
        document.getElementById('questionSection').style.display = 'block';
        document.getElementById('question').style.display = 'flex';
    });

    connection.on("StartGame", function () {
        console.log("Game has started!");
        document.getElementById('questionSection').style.display = 'block';
        document.getElementById('question').style.display = 'flex';
        getNextQuestion();
    });

    connection.on("EndGame", function (results) {
        document.getElementById('questionSection').style.display = 'none';
        document.getElementById('question').style.display = 'none';

        const finishLevel = document.querySelector('.finish__level');
        finishLevel.innerHTML = '';

        let resultText = '<div class="finish__text">Победа!</div>';

        for (let [player, score] of Object.entries(results)) {
            resultText += `<div class="finish__level">Игрок: ${player}, Счет: ${score}</div>`;
        }

        let totalCorrectAnswers = Object.values(results).reduce((acc, cur) => acc + cur, 0);

        resultText += `<div class="correct__answ">Вы ответили на ${totalCorrectAnswers} верно!</div>` +
            '<a href="/Home/SelectMode" class="return__button">Главное меню</a>';

        finishLevel.innerHTML = resultText;
        console.log('Game Ended:', results);

        clearInterval(timerInterval);
        timerInterval = null;

        console.log('END');

        // Trigger confetti animation
        new confettiKit({
            confettiCount: 40,
            angle: 60,
            startVelocity: 80,
            colors: randomColor({ hue: 'blue', count: 18 }),
            elements: {
                'confetti': {
                    direction: 'down',
                    rotation: true,
                },
                'star': {
                    count: 10,
                    direction: 'down',
                    rotation: true,
                },
                'ribbon': {
                    count: 5,
                    direction: 'down',
                    rotation: true,
                },
                'custom': [{
                    count: getRandomInt(2, 4),
                    width: 50,
                    textSize: 15,
                    content: '//bootstraptema.ru/snippets/effect/2018/confettikit/shar.png',
                    contentType: 'image',
                    direction: 'up',
                    rotation: false,
                }]
            },
            position: 'bottomLeftRight',
        });
    });

    // Helper function to get random integer between min and max (inclusive)
    function getRandomInt(min, max) {
        min = Math.ceil(min);
        max = Math.floor(max);
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    async function joinDuel() {
        if (isJoining) return; // Предотвращаем множественные попытки подключения
        isJoining = true;

        try {
            if (connection.state !== 'Disconnected') {
                await connection.stop(); // Останавливаем текущее соединение, если оно уже активно или подключается
            }
            await connection.start(); // Запускаем новое соединение
            var userNameDuel;
            connection.invoke("GetUserName").then(function (userName) {
                console.log("UserName from server:", userName);
                userNameDuel = userName;
            }).catch(function (err) {
                console.error("Error getting UserName:", err);
            });
            console.log(userNameDuel);
            chatRoom = prompt("Enter room name");
            if (!userName || !chatRoom) {
                alert("Username and room name are required.");
                isJoining = false;
                return;
            }
            await connection.invoke("JoinDuel", { UserName: userName, ChatRoom: chatRoom }); // Вызываем метод JoinDuel на сервере
            console.log("Connected and joined duel");
        } catch (err) {
            console.error("Error in joinDuel:", err);
        } finally {
            isJoining = false; // Сбрасываем флаг после завершения попытки подключения
        }
    }


    async function getNextQuestion() {
        try {
            const questionId = parseInt(document.getElementById('questionText').getAttribute('data-question-id')) || 0;
            console.log(`Requesting next question: index=${questionId}`);
            await connection.invoke("GetNextQuestion", userName, chatRoom, questionId);
        } catch (err) {
            console.error(err);
        }
    }

    async function submitAnswer(questionId, answer) {
        try {
            console.log(`Submitting answer: UserName=${userName}, ChatRoom=${chatRoom}, QuestionId=${questionId}, Answer=${answer}`);
            await connection.invoke("AnswerQuestion", userName, chatRoom, questionId, answer);
        } catch (err) {
            console.error("Error invoking AnswerQuestion:", err);
        }
    }

    async function endGame() {
        try {
            await connection.invoke("EndGame", chatRoom); // Ensure chatRoom is passed
        } catch (err) {
            console.error("Error invoking EndGame:", err);
        }
    }

    // Timer function
    function startTimer(durationInSeconds) {
        console.log("Timer started for", durationInSeconds, "seconds");
        const timerBar = document.getElementById('timerBar');
        const totalWidth = document.getElementById('timeBarContainer').offsetWidth;

        // Инициализируем начальное состояние
        timerBar.style.width = totalWidth + 'px';

        let currentTime = durationInSeconds;

        timerInterval = setInterval(() => {
            currentTime -= 1;
            const newWidth = (currentTime / durationInSeconds) * totalWidth;
            timerBar.style.width = newWidth + 'px';

            if (currentTime <= 0) {
                clearInterval(timerInterval);
                timerBar.style.width = '0px';
                endGame(); // Call end game function
            }
        }, 1000);
    }

    document.getElementById('joinDuelBtn').addEventListener('click', joinDuel);

    connection.on("RoomFull", function () {
        alert("Комната заполнена!");
        window.location.reload();
    });
});
