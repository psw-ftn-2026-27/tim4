import { Component, OnInit, input, output, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { SocialChallengeDetailsDto } from '../../api/games-api-types';

@Component({
  imports: [FormField],
  selector: 'app-social-challenge-form',
  templateUrl: './social-challenge-form.html',
  styleUrl: './social-challenge-form.scss',
})
export class SocialChallengeForm implements OnInit {
  readonly initial = input<SocialChallengeDetailsDto>();
  readonly pending = input(false);
  readonly error = input<string | null>(null);
  readonly save = output<SocialChallengeDetailsDto>();

  private readonly model = signal({
    name: '',
    start: '',
    end: '',
    requiredParticipants: null as number | null,
    rangeInMeters: null as number | null,
    xpReward: null as number | null,
    completionMessage: '',
  });

  protected readonly fields = form(this.model, (path) => {
    required(path.name);
    required(path.start);
    required(path.end);
    required(path.requiredParticipants);
    required(path.rangeInMeters);
    required(path.xpReward);
    required(path.completionMessage);
    min(path.requiredParticipants, 2, { message: 'Potrebna su najmanje dva učesnika.' });
    min(path.xpReward, 1, { message: 'XP nagrada mora biti veća od nule.' });
    validate(path.requiredParticipants, ({ value }) =>
      value() === null || Number.isInteger(value())
        ? null
        : { kind: 'integer', message: 'Broj učesnika mora biti ceo broj.' },
    );
    validate(path.xpReward, ({ value }) =>
      value() === null || Number.isInteger(value())
        ? null
        : { kind: 'integer', message: 'XP nagrada mora biti ceo broj.' },
    );
    validate(path.rangeInMeters, ({ value }) =>
      value() === null || Number(value()) > 0
        ? null
        : { kind: 'positive', message: 'Opseg u metrima mora biti veći od nule.' },
    );
  });

  ngOnInit(): void {
    const initial = this.initial();
    if (initial) {
      this.model.set({
        name: initial.name,
        start: initial.start,
        end: initial.end,
        requiredParticipants: initial.requiredParticipants,
        rangeInMeters: initial.rangeInMeters,
        xpReward: initial.xpReward,
        completionMessage: initial.completionMessage,
      });
    }
  }

  protected submit(event: Event): void {
    event.preventDefault();
    const { name, start, end, requiredParticipants, rangeInMeters, xpReward, completionMessage } =
      this.model();
    if (
      !this.fields().valid() ||
      this.pending() ||
      requiredParticipants === null ||
      rangeInMeters === null ||
      xpReward === null
    ) {
      return;
    }

    this.save.emit({
      name,
      start: start.length === 5 ? `${start}:00` : start,
      end: end.length === 5 ? `${end}:00` : end,
      requiredParticipants,
      rangeInMeters,
      xpReward,
      completionMessage,
    });
  }
}
