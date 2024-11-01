package com.drunkengames.hangover.domain.rank.dto;

import com.drunkengames.hangover.domain.rank.entity.Rank;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;
import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;
import org.hibernate.validator.constraints.Length;

@AllArgsConstructor
@NoArgsConstructor
@Getter
@Setter
public class RankRequestDto {
    @NotBlank
    @Length(min = 1, max = 100, message = "닉네임은 1~100자입니다.")
    private String userNickname;

    @Max(value = Integer.MAX_VALUE, message = "최종 자금은 INT 범위입니다.")
    @Min(value = 0, message = "최종 자금은 0 이상 입니다.")
    private Integer finalMoney;

    @Max(value = Integer.MAX_VALUE, message = "최종 일차는 INT 범위입니다.")
    @Min(value = 1, message = "최종 일차는 1 이상 입니다.")
    private Integer finalDay;

    public Rank toEntity() {
        return Rank.builder()
                .userNickname(this.userNickname)
                .finalMoney(this.finalMoney)
                .finalDay(this.finalDay)
                .build();
    }

}
