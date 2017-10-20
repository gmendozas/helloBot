using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using AdaptiveCards;

namespace HelloBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        enum TypeReply { Simple = 1, Speak = 2, Spoken = 3, Both = 4 }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var reply = activity.CreateReply();
            var typeReply = ChooseReply(activity.Text, ref reply);

            if (typeReply == (int) TypeReply.Speak) 
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                await connector.Conversations.SendToConversationAsync(reply);
            } else if(typeReply == (int) TypeReply.Spoken) 
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                await connector.Conversations.ReplyToActivityAsync(reply);
            } else if(typeReply == (int) TypeReply.Both) 
            {
                await context.SayAsync(speak: reply.Speak, text: reply.Text);
            } else 
                await context.PostAsync(reply);

            context.Wait(MessageReceivedAsync);
        }

        private int ChooseReply(string text, ref Activity reply)
        {
            int type = -1;

            try
            {
                if (text.ToLower().Contains("color")) // CardAction
                {
                    reply.Text = "Tengo algunos colores en mente pero necesito que me ayudes a elegir el mejor.";
                    reply.Type = ActivityTypes.Message;
                    reply.TextFormat = TextFormatTypes.Plain;

                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                        {
                            new CardAction(){ Title = "Azul", Type=ActionTypes.ImBack, Value="Azul" },
                            new CardAction(){ Title = "Rojo", Type=ActionTypes.ImBack, Value="Rojo" },
                            new CardAction(){ Title = "Verde", Type=ActionTypes.ImBack, Value="Verde" }
                        }
                    };

                    type = (int)TypeReply.Simple;
                }
                else if (text.ToLower().Contains("hambre")) // Carousel: CardImage + CardAction + HeroCard
                {
                    reply.Text = "Deberíamos checar alguna de estas opciones, ¿qué opinas?";
                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments = new List<Attachment>();

                    Dictionary<string, string> cardContentList = new Dictionary<string, string>();
                    cardContentList.Add("Pig Latin", "http://www.ccalanguagesolutions.com/wp-content/uploads/2013/12/pig-300x248.jpg");
                    cardContentList.Add("Paleta de cerdo", "https://www.kingsford.com/wp-content/uploads/2014/12/kfd-howtoporkshoulder-PorkShoulder5_0241-1024x621.jpg");
                    cardContentList.Add("Tocino", "http://www.animalgourmet.com/wp-content/uploads/2014/05/tocino.jpg");

                    foreach(KeyValuePair<string, string> cardContent in cardContentList)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url:cardContent.Value ));

                        List<CardAction> cardButtons = new List<CardAction>();

                        CardAction plButton = new CardAction()
                        {
                            Value = string.Format("https://es.wikipedia.org/wiki/{0}", cardContent.Key),
                            Type = "openUrl",
                            Title = "WikiPedia Page"
                        };

                        cardButtons.Add(plButton);

                        HeroCard plCard = new HeroCard()
                        {
                            Title = string.Format("Que tal si vemos algo sobre {0}", cardContent.Key),
                            Subtitle = string.Format("{0} Wikipedia Page", cardContent.Key),
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply.Attachments.Add(plAttachment);
                    }

                    type = (int)TypeReply.Simple;
                } else if(text.ToLower().Contains("anime")) // Thumbnail
                {
                    reply.Text = "Conozco un par, ¿qué opinas de estas opciones?";
                    reply.AttachmentLayout = AttachmentLayoutTypes.List;
                    reply.Attachments = new List<Attachment>();

                    Dictionary<string, string> cardContentList = new Dictionary<string, string>();
                    cardContentList.Add("Shingeki no Kyojin", "https://d3ieicw58ybon5.cloudfront.net/ex/200.200/shop/product/6187052367c5437aa92201f334472226.jpg");
                    cardContentList.Add("Dragon Ball", "http://picture-cdn.wheretoget.it/5/7/57djqn.jpg");

                    foreach(KeyValuePair<string, string> cardContent in cardContentList)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url:cardContent.Value ));

                        List<CardAction> cardButtons = new List<CardAction>();

                        CardAction plButton = new CardAction()
                        {
                            Value = string.Format("https://es.wikipedia.org/wiki/{0}", cardContent.Key),
                            Type = "openUrl",
                            Title = "WikiPedia Page"
                        };

                        cardButtons.Add(plButton);

                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = string.Format("Veamos un poco sobre {0}" ,cardContent.Key),
                            Subtitle = string.Format("{0} Wikipedia Page", cardContent.Key),
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply.Attachments.Add(plAttachment);
                    }

                    type = (int)TypeReply.Simple;
                }
                else if (text.ToLower().Contains("compras")) // Receipt card
                {
                    reply.Text = "Vamos de compras, aquí hay algunas opciones, ¿qué opinas?";
                    reply.Attachments = new List<Attachment>();

                    //List<CardImage> cardImages = new List<CardImage>();
                    //cardImages.Add(new CardImage(url: "https://<imageUrl1>" ));

                    List<CardAction> cardButtons = new List<CardAction>();

                    CardAction plButton = new CardAction()
                    {
                        Value = "https://es.wikipedia.org/wiki/Pig_Latin",
                        Type = "openUrl",
                        Title = "WikiPedia Page"
                    };

                    cardButtons.Add(plButton);

                    ReceiptItem lineItem1 = new ReceiptItem()
                    {
                        Title = "Paleta de Cerdo",
                        Subtitle = "8 lbs",
                        Text = null,
                        Image = new CardImage(url: "https://www.kingsford.com/wp-content/uploads/2014/12/kfd-howtoporkshoulder-PorkShoulder5_0241-1024x621.jpg"),
                        Price = "16.25",
                        Quantity = "1",
                        Tap = null
                    };

                    ReceiptItem lineItem2 = new ReceiptItem()
                    {
                    Title = "Tocino",
                    Subtitle = "5 lbs",
                    Text = null,
                    Image = new CardImage(url: "http://www.animalgourmet.com/wp-content/uploads/2014/05/tocino.jpg"),
                    Price = "34.50",
                    Quantity = "2",
                    Tap = null
                    };

                    List<ReceiptItem> receiptList = new List<ReceiptItem>();
                    receiptList.Add(lineItem1);
                    receiptList.Add(lineItem2);

                    ReceiptCard plCard = new ReceiptCard()
                    {
                        Title = "¿No es este tocino muy caro?",
                        Buttons = cardButtons,
                        Items = receiptList,
                        Total = "112.77",
                        Tax = "27.52"
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    reply.Attachments.Add(plAttachment);

                    type = (int)TypeReply.Simple;
                } else if(text.ToLower().Contains("contactar"))  // Sign-In Card
                {
                    reply.Text = "Hay que hacer algo antes de eso.";
                    reply.Attachments = new List<Attachment>();

                    List<CardAction> cardButtons = new List<CardAction>();

                    CardAction plButton = new CardAction()
                    {
                        Value = "https://login.microsoftonline.com/common/federation/oauth2",
                        Type = "signin",
                        Title = "Conectar"
                    };

                    cardButtons.Add(plButton);

                    SigninCard plCard = new SigninCard("Primero necesitas autorizarme.", cardButtons);

                    Attachment plAttachment = plCard.ToAttachment();
                    reply.Attachments.Add(plAttachment);

                    type = (int)TypeReply.Simple;
                } else if(text.ToLower().Contains("cita")) // Adaptative Card
                {
                    reply.Text = "Veamos, me parece que tienes algo pendiente:";
                    reply.Attachments = new List<Attachment>();

                    AdaptiveCard card = new AdaptiveCard();

                    // Specify speech for the card.
                    var hourOfDay = DateTime.Now;

                    card.Speak = string.Concat("<s>Tu reunión sobre \"Adaptive Card design session\"<break strength='weak'/>"
                        , string.Format("empieza a las {0} ", hourOfDay.ToString("HH:mm"))
                        , "</s><s>¿Quieres aplazarla <break strength='weak'/> o quieres avisar que llegarás tarde?</s>");

                    // Add text to the card.
                    card.Body.Add(new TextBlock()
                    {
                        Text = "Adaptive Card design session",
                        Size = TextSize.Large,
                        Weight = TextWeight.Bolder
                    });

                    // Add text to the card.
                    card.Body.Add(new TextBlock()
                    {
                        Text = "Conf Room 112/3377 (10)"
                    });

                    // Add text to the card.
                    card.Body.Add(new TextBlock()
                    {
                        Text = string.Format("{0} - {1}", hourOfDay.ToString("HH:mm"), hourOfDay.AddHours(1).ToString("HH:mm"))
                    });

                    // Add list of choices to the card.
                    card.Body.Add(new ChoiceSet()
                    {
                        Id = "snooze",
                        Style = ChoiceInputStyle.Compact,
                        Choices = new List<Choice>()
                        {
                            new Choice() { Title = "5 minutos", Value = "5", IsSelected = true },
                            new Choice() { Title = "15 minutos", Value = "15" },
                            new Choice() { Title = "30 minutos", Value = "30" }
                        }
                    });

                    // Add buttons to the card.
                    card.Actions.Add(new ActionBase()
                    {                        
                        Title = "Silenciar"
                    });

                    card.Actions.Add(new HttpAction()
                    {
                        Url = "http://foo.com",
                        Title = "Llegaré Tarde"
                    });

                    card.Actions.Add(new HttpAction()
                    {
                        Url = "http://foo.com",
                        Title = "Ignorar"
                    });

                    // Create the attachment.
                    Attachment attachment = new Attachment()
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = card
                    };

                    reply.Attachments.Add(attachment);

                    type = (int)TypeReply.Speak;
                }
                else if (text.ToLower().Contains("hello")) // Adaptative Card simple
                {
                    AdaptiveCard card = new AdaptiveCard();

                    card.Body.Add(new TextBlock() 
                    {
                        Text = "Hello",
                        Size = TextSize.ExtraLarge
                    });

                    card.Body.Add(new Image() 
                    {
                        Url = "https://lh6.ggpht.com/LCpP0kIk5XubEwBUhOomMFgees_ALTxUvfDjMfrnwaSxa9Khw50d0Lw0EW7DFs85KXQ=w300"
                    });

                    // Create the attachment.
                    Attachment attachment = new Attachment()
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = card
                    };

                    reply.Attachments.Add(attachment);

                    type = (int)TypeReply.Simple;
                } else if(text.ToLower().Contains("hablar")) // Speak que acepta inputs
                {
                    reply.Text = "Veamos si puedes escucharme.";
                    reply.Speak = "¿Ya me escuchas?";
                    reply.InputHint = InputHints.AcceptingInput;

                    type = (int)TypeReply.Spoken;
                } else if(text.ToLower().Contains("parlar")) // Speak que dice y escribe lo mismo
                {
                    reply.Text = "Esta es una respuesta que puedes leer y escuchar";
                    reply.Speak = "Esta es una respuesta que puedes leer y escuchar";

                    type = (int)TypeReply.Both;
                } else // PlainText
                {
                    int length = (text ?? string.Empty).Length;
                    reply.Text = string.Format("Enviaste \"{0}\" con una longitud de {1}.", text, length);
                    type = (int)TypeReply.Simple;
                }
            } catch(Exception ex) 
            {
                reply.Text = "No entendí tu respuesta, volvamos a intentarlo.";
                type = (int)TypeReply.Simple;
            }

            return type;
        }
    }
}