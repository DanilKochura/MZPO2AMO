﻿using MZPO.AmoRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    internal static class TagList
    {
        private static List<Tag> tags = new()
        {
            new() { id = 139175, name = "Заявка с сайта" },
            new() { id = 203033, name = "insta" },
            new() { id = 217501, name = "whatsapp" },
            new() { id = 246111, name = "skillbank.su" },
            new() { id = 246137, name = "mzpo-s.ru" },
            new() { id = 246139, name = "Обратный звонок" },
            new() { id = 246141, name = "mzpokurs.com" },
            new() { id = 246165, name = "mirk.msk.ru" },
            new() { id = 246169, name = "Заявки из корзины" },
            new() { id = 246171, name = "mzpo.education" },
            new() { id = 246195, name = "cruche-academy.ru" },
            new() { id = 246263, name = "Оставьте заявку" },
            new() { id = 246292, name = "Коллтрекинг" },
            new() { id = 246296, name = "Обратная связь" },
            new() { id = 246481, name = "JivoSite" },
            new() { id = 247089, name = "Консультация (БП)" },
            new() { id = 247149, name = "Кузнецкий мост" },
            new() { id = 247157, name = "Пушкинская" },
            new() { id = 247161, name = "Менделеевская" },
            new() { id = 247887, name = "Прямой звонок" },
            new() { id = 250295, name = "Емейлтрекинг" },
            new() { id = 260779, name = "Академ" },
            new() { id = 261313, name = "Допродажа" },
            new() { id = 261321, name = "Повторная продажа" },
            new() { id = 261879, name = "obr-mast.ru" },
            new() { id = 263379, name = "inst-s.ru" },
            new() { id = 264021, name = "WZ (mzpokurs.com_WA)" },
            new() { id = 264023, name = "WZ (mirk.msk_WA)" },
            new() { id = 264027, name = "WZ (cruche-academy_WA)" },
            new() { id = 264031, name = "WZ (mzpo.education_WA)" },
            new() { id = 264061, name = "WZ (mzpo-s.ru_WA)" },
            new() { id = 264265, name = "WZ (skillbank_WA)" },
            new() { id = 264293, name = "курсы-сестринского-дела.рф" },
            new() { id = 264383, name = "Ручная" },
            new() { id = 264863, name = "vkontakte" },
            new() { id = 265395, name = "telegram" },
            new() { id = 265935, name = "obr-byx.ru" },
            new() { id = 265941, name = "zoon" },
            new() { id = 266015, name = "2gis" },
            new() { id = 266771, name = "Отложенный спрос" },
            new() { id = 267095, name = "Актуализация" },
            new() { id = 267135, name = "Взято в работу" },
            new() { id = 267137, name = "Отправлено КП" },
            new() { id = 267301, name = "obr-men.ru" },
            new() { id = 267303, name = "obr-komp.ru" },
            new() { id = 267307, name = "obr-sek.ru" },
            new() { id = 267309, name = "obr-diz.ru" },
            new() { id = 267355, name = "Вызревание" },
            new() { id = 267359, name = "Распределение" },
            new() { id = 267417, name = "Промоутер" },
            new() { id = 268655, name = "NPS" },
            new() { id = 268981, name = "8-11" },
            new() { id = 269475, name = "mirk.vrach.kosmetolog" },
            new() { id = 269611, name = "Обзвон" },
            new() { id = 269973, name = "mzpo2amo" },
            new() { id = 270683, name = "facebook" },
            new() { id = 271285, name = "Рассылка логопедический массаж" },
            new() { id = 271741, name = "Рассылка фитнес-инструктор" },
            new() { id = 272529, name = "Акция" },
            new() { id = 272921, name = "Коллтрекинг mzpo.education" },
            new() { id = 273127, name = "ucheba.ru" },
            new() { id = 273359, name = "Вебинар НМО неврология" },
            new() { id = 273375, name = "_test" },
            new() { id = 273459, name = "ДОД" },
            new() { id = 273475, name = "WZ (mzpo-s)" },
            new() { id = 275147, name = "WZ (mirkmsk)" },
            new() { id = 275165, name = "FB_vrach-kosmetolog" },
            new() { id = 275191, name = "1C" },
            new() { id = 275327, name = "!I" },
            new() { id = 275451, name = "WeGym" },
            new() { id = 275555, name = "Тайский массаж мешочками" },
            new() { id = 275839, name = "WZ (OS_1_WA)" },
            new() { id = 275841, name = "WZ (OS_2_WA)" },
            new() { id = 276829, name = "Вебинар" },
            new() { id = 276831, name = "Мероприятие" },
            new() { id = 277455, name = "WZ (mzpokurs)" },
            new() { id = 300469, name = "turbo.mirk.msk" },
            new() { id = 300471, name = "turbo.mirk.msk" },

            new() { id = 302489, name = "Сделка из корп. отдела" },
            new() { id = 1212821, name = "Сделка из розницы" },
        };

        public static Tag GetTagByName(string name)
        {
            if (!tags.Any(x => x.name == name)) return null;

            return tags.First(x => x.name == name);
        }

        public static Tag GetTagByid(int id)
        {
            if (!tags.Any(x => x.id == id)) return null;

            return tags.First(x => x.id == id);
        }
    }
}