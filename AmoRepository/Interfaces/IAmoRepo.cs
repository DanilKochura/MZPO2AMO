﻿using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Interface of an amoCRM repository.
    /// </summary>
    /// <typeparam name="T">Entity in amoCRM, must implement <see cref="IEntity"/></typeparam>
    public interface IAmoRepo<T> where T: IEntity, new()
    {
        /// <summary>
        /// Добавляет в amoCRM новые сущности. Принимает список сущностей. Возвращает список добавленных сущностей.
        /// </summary>
        /// <param name="payload">Список сущностей.</param>
        /// <returns>Список добавленных сущностей.</returns>
        public IEnumerable<T> AddNew(IEnumerable<T> payload);

        /// <summary>
        /// Добавляет в amoCRM новую сущность. Принимает объект сущности. Возвращает список, содержащий добавленную сущность.
        /// </summary>
        /// <param name="payload">Список сущностей.</param>
        /// <returns>Список добавленных сущностей.</returns>
        public IEnumerable<T> AddNew(T payload);

        /// <summary>
        /// Возвращает из amoCRM список сущностей, запрошенных по критерию.
        /// </summary>
        /// <param name="criteria">Критерии поиска.</param>
        /// <returns>Список найденных сущностей.</returns>
        public IEnumerable<T> GetByCriteria(string criteria);

        /// <summary>
        /// Возвращает из amoCRM список сущностей. Принимает список id сущностей, запрашивает пакетно по 10 штук. 
        /// </summary>
        /// <param name="ids">Список id сущностей.</param>
        /// <returns>Список найденных сущностей.</returns>
        public IEnumerable<T> BulkGetById(IEnumerable<int> ids);

        /// <summary>
        /// Возвращает из amoCRM сущность по id, если сущность не найдена, возвращает null.
        /// </summary>
        /// <param name="ids">Id сущностей.</param>
        /// <returns>Объект найденной сущности или null.</returns>
        public T GetById(int id);

        /// <summary>
        /// Сохраняет в amoCRM измененные сущности. Принимает список сущностей. Возвращает список измененных сущностей.
        /// </summary>
        /// <param name="payload">Список сущностей.</param>
        /// <returns>Список измененных сущностей.</returns>
        public IEnumerable<T> Save(IEnumerable<T> payload);

        /// <summary>
        /// Сохраняет в amoCRM измененную сущность. Принимает объект сущности. Возвращает список, содержащий измененную сущность.
        /// </summary>
        /// <param name="payload">Объект сущности.</param>
        /// <returns>Список содержащий измененную сущность.</returns>
        public IEnumerable<T> Save(T payload);

        /// <summary>
        /// Возвращает из amoCRM список событий для сущности.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <returns>Список событий сущности.</returns>
        public IEnumerable<Event> GetEntityEvents(int id);

        /// <summary>
        /// Возвращает из amoCRM список событий по критерию.
        /// </summary>
        /// <param name="criteria">Критерии поиска.</param>
        /// <returns>Список найденных событий.</returns>
        public IEnumerable<Event> GetEventsByCriteria(string criteria);

        /// <summary>
        /// Возвращает из amoCRM список примечаний к сущности.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <returns>Список примечаний к сущности.</returns>
        public IEnumerable<Note> GetEntityNotes(int id);

        /// <summary>
        /// Возвращает из amoCRM список примечаний по критерию.
        /// </summary>
        /// <param name="criteria">Критерии поиска.</param>
        /// <returns>Список найденных примечаний.</returns>
        public IEnumerable<Note> GetNotesByCriteria(string criteria);

        /// <summary>
        /// Возвращает из amoCRM конкретное примечание. Возвращает null если примечание не найдено.
        /// </summary>
        /// <param name="id">Id причания.</param>
        /// <returns>Объект найденного примечания или null.</returns>
        public Note GetNoteById(int id);

        /// <summary>
        /// Возвращает из amoCRM список примечаний. Принимает список id примечаний. Запрашивает пакетно по 10 штук.
        /// </summary>
        /// <param name="ids">Список id примечаний.</param>
        /// <returns>Список найденных примечаний.</returns>
        public IEnumerable<Note> BulkGetNotesById(IEnumerable<int> ids);

        /// <summary>
        /// Добавляет к сущности amoCRM примечание. Принимает id сущности и текст примечания. Возвращает список, содержащий добавленное примечание.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <param name="comment">Текст примечания.</param>
        /// <returns>Список содержащий добавленное примечание.</returns>
        public IEnumerable<Note> AddNotes(int id, string comment);

        /// <summary>
        /// Добавляет в amoCRM примечание. Принимает объект примечания. Возвращает список, содержащий добавленное примечание.
        /// </summary>
        /// <param name="note">Объект примечания.</param>
        /// <returns>Список содержащий добавленное примечание.</returns>
        public IEnumerable<Note> AddNotes(Note note);

        /// <summary>
        /// Добавляет в amoCRM примечания. Принимает список примечаний. Возвращает список добавленных примечаний.
        /// </summary>
        /// <param name="payload">Список примечаний.</param>
        /// <returns>Список содержащий добавленные примечания.</returns>
        public IEnumerable<Note> AddNotes(IEnumerable<Note> payload);

        /// <summary>
        /// Возвращает из amoCRM список тегов для сущности.
        /// </summary>
        /// <returns>Список тегов сущности.</returns>
        public IEnumerable<Tag> GetTags();

        /// <summary>
        /// Добавляет список тегов для сущности в amoCRM. Принимает список тегов. Возвращает список добавленных тегов.
        /// </summary>
        /// <param name="payload">Список тегов.</param>
        /// <returns>Список содержащий добавленные теги.</returns>
        public IEnumerable<Tag> AddTag(IEnumerable<Tag> payload);

        /// <summary>
        /// Добавляет к сущности amoCRM тег. Принимает объект тега. Возвращает список, содержащий добавленный тег.
        /// </summary>
        /// <param name="newTag">Объект тега.</param>
        /// <returns>Список содержащий добавленный тег.</returns>
        public IEnumerable<Tag> AddTag(Tag newTag);

        /// <summary>
        /// Добавляет к сущности amoCRM тег. Принимает название тега. Возвращает список, содержащий добавленный тег.
        /// </summary>
        /// <param name="tagName">Название тега.</param>
        /// <returns>Список содержащий добавленный тег.</returns>
        public IEnumerable<Tag> AddTag(string tagName);

        /// <summary>
        /// Возвращает из amoCRM список дополнительных полей сущности.
        /// </summary>
        /// <returns>Список дополнительных полей сущности.</returns>
        public IEnumerable<CustomField> GetFields();

        /// <summary>
        /// Добавляет к сущности amoCRM дополнительные поля. Принимает список дополнительных полей. Возвращает список добавленных полей.
        /// </summary>
        /// <param name="payload">Список дополнительных полей.</param>
        /// <returns>Список содержащий добавленные поля.</returns>
        public IEnumerable<CustomField> AddField(IEnumerable<CustomField> payload);

        /// <summary>
        /// Добавляет к сушности amoCRM дополнительное поле. Принимает объект дополнительного поля. Возвращает список, содержащий дополнительное поле.
        /// </summary>
        /// <param name="customField">Объект дополнительного поля.</param>
        /// <returns>Список содержащий добавленное поле.</returns>
        public IEnumerable<CustomField> AddField(CustomField customField);

        /// <summary>
        /// Добавляет к сущности amoCRM дополнительное текстовое поле. Принимает название поля. Возвращает список, содержащий добавленное поле.
        /// </summary>
        /// <param name="fieldName">Название дополнительного поля.</param>
        /// <returns>Список содержащий добавленное поле.</returns>
        public IEnumerable<CustomField> AddField(string fieldName);

        /// <summary>
        /// Принимает в amoCRM Неразобранное по идентификатору.
        /// </summary>
        /// <param name="uid">Идентификатор Неразобранного в amoCRM.</param>
        public void AcceptUnsorted(string uid);
    }
}