// Управляет повествованием игры: вступлением, финалом, мыслями, фразами и Гостем
using UnityEngine;
using System;
using System.Collections.Generic;

public class NarrativeManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager; // Ссылка на UI-менеджер
    [SerializeField] private PlayerController playerController; // Ссылка на игрока

    private readonly string introText = 
        "Солас был бродячим торговцем, чья жизнь была полна приключений. " +
        "Он странствовал по пыльным дорогам и зеленым долинам, торгуя шелками с востока, " +
        "специями с юга и древними артефактами из забытых руин. " +
        "Его повозка, запряженная старым мулом по кличке Брин, была его домом, " +
        "а звезды — единственными спутниками в долгих ночах. " +
        "Солас любил рассказывать истории, и его улыбка согревала сердца в тавернах. " +
        "Но во время последнего путешествия через густой лес на его караван напали. " +
        "Тени мелькали между деревьями, звон стали смешался с криками, и тьма поглотила его. " +
        "Теперь Солас очнулся в холодном подземелье, окруженный каменными стенами и страхом. " +
        "Чтобы выжить, ему нужно продержаться 10 дней в этом лабиринте. " +
        "Нажмите Пробел, чтобы начать...";

    private string GetFinalText()
    {
        string baseText = 
            "После 10 дней борьбы, голода и страха Солас нашел выход. " +
            "Он поднялся по каменным ступеням, и первые лучи рассвета ослепили его. " +
            "Свежий ветер коснулся его лица, а перед ним раскинулся бескрайний луг, искрящийся росой. " +
            "Впервые за долгое время Солас почувствовал надежду. " +
            "Он пережил кошмар и теперь верит в прекрасное будущее. " +
            "С улыбкой он шагает вперед, готовый к новой главе своей жизни.";
        if (GameManager.Instance.HasSeenCross())
        {
            baseText += " Крест на полу... Он все еще в моих мыслях. Что он означал?";
        }
        return baseText;
    }

    private readonly string guestDialogue = 
        "Ты... Живой? Я здесь около месяца... Не помню, кем был до этого подземелья. " +
        "Имя? Оно стерлось. Остались только тени воспоминаний. " +
        "Этот красный крест на мне... Он жжет, мучает. Не могу избавиться. " +
        "Иногда кажется, что он шепчет... Следи за собой, путник. " +
        "Хочешь совет? Еда — не главное. Иногда лучше голодать, чем рисковать. " +
        "И враги... Они предсказуемы. Изучай их шаги, и они не тронут тебя. " +
        "Удачи, путник. Может, еще свидимся...";

    private readonly string guestEndingPrompt = 
        "Ты выбрался... Я тоже. Не знаю, как. Может, судьба? " +
        "Пойдешь дальше? Возьмешь меня? Я не обуза, клянусь.";

    private string GetGuestEndingAgree()
    {
        string baseText = 
            "Солас кивнул. Двое путников шагнули в рассвет, их тени слились на траве. " +
            "Новое приключение ждало их.";
        if (GameManager.Instance.HasSeenCross())
        {
            baseText += " Крест на его плаще и на полу... Это не случайность.";
        }
        return baseText;
    }

    private string GetGuestEndingRefuse()
    {
        string baseText = 
            "Солас покачал головой. Гость кивнул, не сказав ни слова, и исчез в утреннем тумане. " +
            "Солас пошел дальше один.";
        if (GameManager.Instance.HasSeenCross())
        {
            baseText += " Крест на его плаще и на полу... Это не случайность.";
        }
        return baseText;
    }

    private readonly string crossThought = 
        "Этот крест... Как у него. Что это значит?";

    private readonly string crossUsedThought = 
        "Крест исчез... Его сила ушла.";

    private readonly List<string> randomThoughts = new List<string>
    {
        "Брин, где ты, старина?",
        "Эти стены... Давят.",
        "Я выберусь, клянусь!",
        "Звезды... Скоро увижу их.",
        "Моя повозка... Все пропало.",
        "Надежда — мой маяк.",
        "Тени следят за мной?",
        "Солас, держись, ты справишься."
    };

    private readonly List<string> guestThoughts = new List<string>
    {
        "Кто он был? Его глаза... Пустые.",
        "Гость знал это место. Как он выжил?",
        "Красный крест... Символ или проклятье?"
    };

    private readonly List<string> crossThoughts = new List<string>
    {
        "Этот гул... Он зовет меня.",
        "Звук креста... Он в моей голове.",
        "Почему этот гул не дает мне покоя?"
    };

    private readonly List<string> helpfulPhrases = new List<string>
    {
        "Еда! Это даст мне силы.",
        "Выход близко, я чувствую!",
        "Осторожно, враги рядом!",
        "Еще немного, и я на свободе."
    };

    public void ShowIntro(Action onComplete)
    {
        uiManager.ShowNarrative(introText, onComplete);
    }

    public void ShowFinal(Action onComplete)
    {
        uiManager.ShowNarrative(GetFinalText(), onComplete);
    }

    public void ShowGuestDialogue()
    {
        uiManager.ShowDialogue(guestDialogue);
    }

    public void ShowGuestEnding()
    {
        uiManager.ShowNarrativeWithChoice(guestEndingPrompt, () => {
            uiManager.ShowNarrative(GetGuestEndingAgree(), () => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
        }, () => {
            uiManager.ShowNarrative(GetGuestEndingRefuse(), () => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
        });
    }

    public void ShowRandomThought()
    {
        List<string> thoughts = new List<string>(randomThoughts);
        if (GameManager.Instance.HasMetGuest())
        {
            thoughts.AddRange(guestThoughts); // Добавляем мысли о Госте
        }
        if (GameManager.Instance.HasSeenCross() && !GameManager.Instance.HasCross() && GameManager.Instance.GetCurrentDay() == 9)
        {
            thoughts.AddRange(crossThoughts); // Добавляем мысли о звуке креста
        }

        if (thoughts.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, thoughts.Count);
            string thought = thoughts[index];
            // Если мысль о Госте или кресте, шанс 30%
            if ((GameManager.Instance.HasMetGuest() && guestThoughts.Contains(thought)) || 
                (GameManager.Instance.HasSeenCross() && crossThoughts.Contains(thought)))
            {
                if (UnityEngine.Random.value > 0.3f)
                {
                    index = UnityEngine.Random.Range(0, randomThoughts.Count); // Выбираем обычную мысль
                    thought = randomThoughts[index];
                }
            }
            playerController.ShowThought(thought);
        }
    }

    public void ShowCrossThought()
    {
        playerController.ShowThought(crossThought);
    }

    public void ShowCrossSoundThought()
    {
        if (crossThoughts.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, crossThoughts.Count);
            playerController.ShowThought(crossThoughts[index]);
        }
    }

    public void ShowCrossUsedThought()
    {
        playerController.ShowThought(crossUsedThought);
    }

    public void ShowHelpfulPhrase(string context = "")
    {
        if (helpfulPhrases.Count > 0)
        {
            string phrase;
            if (context == "Food")
            {
                phrase = helpfulPhrases[0];
            }
            else if (context == "Exit")
            {
                phrase = helpfulPhrases[1];
            }
            else
            {
                int index = UnityEngine.Random.Range(0, helpfulPhrases.Count);
                phrase = helpfulPhrases[index];
            }
            uiManager.ShowDialogue(phrase);
        }
    }
}
