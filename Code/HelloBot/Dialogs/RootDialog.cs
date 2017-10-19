using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace HelloBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var reply = activity.CreateReply();
            ChooseReply(activity.Text, ref reply);

            await context.PostAsync(reply);

            context.Wait(MessageReceivedAsync);
        }

        private void ChooseReply(string text, ref Activity reply)
        {
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
                }
                else if (text.ToLower().Contains("compras")) // Receipt card
                {
                    reply.Text = "Vamos de compras, aquí hay algunas opciones, ¿qué opinas?";
                    reply.Attachments = new List<Attachment>();

                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://<imageUrl1>" ));

                    List<CardAction> cardButtons = new List<CardAction>();

                    CardAction plButton = new CardAction()
                    {
                        Value = "https://en.wikipedia.org/wiki/PigLatin",
                        Type = "openUrl",
                        Title = "WikiPedia Page"
                    };

                    cardButtons.Add(plButton);

                    ReceiptItem lineItem1 = new ReceiptItem()
                    {
                        Title = "Pork Shoulder",
                        Subtitle = "8 lbs",
                        Text = null,
                        Image = new CardImage(url: "https://<ImageUrl1>"),
                        Price = "16.25",
                        Quantity = "1",
                        Tap = null
                    };

                    ReceiptItem lineItem2 = new ReceiptItem()
                    {
                    Title = "Bacon",
                    Subtitle = "5 lbs",
                    Text = null,
                    Image = new CardImage(url: "https://<ImageUrl2>"),
                    Price = "34.50",
                    Quantity = "2",
                    Tap = null
                    };

                    List<ReceiptItem> receiptList = new List<ReceiptItem>();
                    receiptList.Add(lineItem1);
                    receiptList.Add(lineItem2);

                    ReceiptCard plCard = new ReceiptCard()
                    {
                        Title = "I'm a receipt card, isn't this bacon expensive?",
                        Buttons = cardButtons,
                        Items = receiptList,
                        Total = "112.77",
                        Tax = "27.52"
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    reply.Attachments.Add(plAttachment);
                } else // PlainText
                {
                    int length = (text ?? string.Empty).Length;
                    reply.Text = string.Format("Enviaste \"{0}\" con una longitud de {1}.", text, length);
                }
            } catch(Exception ex) 
            {
                reply.Text = "No entendí tu respuesta, volvamos a intentarlo.";
            }
        }
    }
}